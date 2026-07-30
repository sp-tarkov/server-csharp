using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers.Ws.Message;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using Microsoft.Extensions.Logging;

namespace SPTarkov.Server.Core.Servers.Ws;

[Injectable(InjectionType.Singleton)]
public sealed class SptWebSocketConnectionHandler(
    ISptLogger<SptWebSocketConnectionHandler> logger,
    ServerLocalisationService serverLocalisationService,
    JsonUtil jsonUtil,
    ProfileHelper profileHelper,
    IEnumerable<ISptWebSocketMessageHandler> messageHandlers
) : IWebSocketConnectionHandler
{
    private readonly Dictionary<MongoId, Dictionary<string, WebSocket>> _sockets = [];
    private readonly Lock _socketsLock = new();
    private readonly ConcurrentDictionary<WebSocket, SemaphoreSlim> _sendGates = [];

    public string GetHookUrl()
    {
        return "/notifierServer/getwebsocket/";
    }

    public string GetSocketId()
    {
        return "SPT WebSocket Handler";
    }

    public Task OnConnectionAsync(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var sessionID = new MongoId(GetSessionId(context));
        var playerProfile = profileHelper.GetFullProfile(sessionID);
        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Websocket connect for player: {playerInfoText} started with context: {sessionIdContext}");
        }

        lock (_socketsLock)
        {
            if (_sockets.TryGetValue(sessionID, out var sessionSockets))
            {
                PruneClosedSockets(sessionSockets);

                if (sessionSockets.Count != 0)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            serverLocalisationService.GetText(
                                "websocket-player_reconnect",
                                new { sessionId = playerInfoText, contextId = sessionIdContext }
                            )
                        );
                    }
                }
            }
            else
            {
                sessionSockets = [];
                _sockets.Add(sessionID, sessionSockets);
            }

            sessionSockets.Add(sessionIdContext, ws);
            if (logger.IsLogEnabled(LogLevel.Information))
            {
                logger.Info(
                    serverLocalisationService.GetText(
                        "websocket-player_connected",
                        new { sessionId = playerInfoText, contextId = sessionIdContext }
                    )
                );
            }

            return Task.CompletedTask;
        }
    }

    public async Task OnMessageAsync(byte[] receivedMessage, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        var sessionID = GetSessionId(context);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Message for session {sessionID} received. Notifying message handlers.");
        }

        foreach (var sptWebSocketMessageHandler in messageHandlers)
        {
            await sptWebSocketMessageHandler.OnSptMessageAsync(sessionID, ws, receivedMessage);
        }
    }

    public Task OnCloseAsync(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var sessionID = GetSessionId(context);

        lock (_socketsLock)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Attempting to close websocket session {sessionID} with context {sessionIdContext}");
            }

            if (_sockets.TryGetValue(sessionID, out var sessionSockets) && sessionSockets.Count > 0)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Websockets for session {sessionID} entry matched, attempting to find context {sessionIdContext}");
                }

                if (!sessionSockets.TryGetValue(sessionIdContext, out _) && logger.IsLogEnabled(LogLevel.Information))
                {
                    logger.Info(
                        $"[WS] The websocket session {sessionID} with reference: {sessionIdContext} has already been removed or reconnected"
                    );
                }
                else
                {
                    sessionSockets.Remove(sessionIdContext);
                    if (logger.IsLogEnabled(LogLevel.Information))
                    {
                        var playerProfile = profileHelper.GetFullProfile(sessionID);
                        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
                        logger.Info($"[WS] player: {playerInfoText} {sessionIdContext} has disconnected");
                    }
                }

                // Once the last socket for a session goes away, drop the session entry entirely.
                if (sessionSockets.Count == 0)
                {
                    _sockets.Remove(sessionID);
                }
            }
            else
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        $"Websocket for session {sessionID} with context {sessionIdContext} does not exist on the socket map, nothing was removed"
                    );
                }
            }
        }

        // Release the per-socket send gate now that the connection is gone.
        if (_sendGates.TryRemove(ws, out var gate))
        {
            gate.Dispose();
        }

        return Task.CompletedTask;
    }

    public Task SendMessageToAll(WsNotificationEvent output)
    {
        // Serialize once and reuse the payload for every socket rather than re-serializing per session.
        var payload = Encoding.UTF8.GetBytes(
            jsonUtil.Serialize(output, output.GetType()) ?? throw new InvalidOperationException("Could not serialize message!")
        );

        WebSocket[] targets;
        lock (_socketsLock)
        {
            targets = _sockets.Values.SelectMany(sockets => sockets.Values).Where(s => s.State == WebSocketState.Open).ToArray();
        }

        return SendRawToSocketsAsync(targets, payload);
    }

    public Task SendMessageAsync(MongoId sessionID, WsNotificationEvent output)
    {
        WebSocket[] targets;
        lock (_socketsLock)
        {
            targets = _sockets.GetValueOrDefault(sessionID)?.Values.Where(s => s.State == WebSocketState.Open).ToArray() ?? [];
        }

        if (targets.Length == 0)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(serverLocalisationService.GetText("websocket-not_ready_message_not_sent", sessionID.ToString()));
            }

            return Task.CompletedTask;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Send message for {sessionID} matched {targets.Length} websockets. Messages being sent");
        }

        var payload = Encoding.UTF8.GetBytes(
            jsonUtil.Serialize(output, output.GetType()) ?? throw new InvalidOperationException("Could not serialize message!")
        );

        return SendRawToSocketsAsync(targets, payload);
    }

    private async Task SendRawToSocketsAsync(WebSocket[] sockets, byte[] payload)
    {
        foreach (var webSocket in sockets)
        {
            var gate = _sendGates.GetOrAdd(webSocket, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                await webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(serverLocalisationService.GetText("websocket-message_sent"));
                }
            }
            catch (Exception err)
            {
                logger.Error(serverLocalisationService.GetText("websocket-message_send_failed_with_error", err.Message), err);
            }
            finally
            {
                gate.Release();
            }
        }
    }

    public bool IsWebSocketConnected(MongoId sessionID)
    {
        lock (_socketsLock)
        {
            return _sockets.TryGetValue(sessionID, out var sockets) && sockets.Any(s => s.Value.State == WebSocketState.Open);
        }
    }

    public IEnumerable<WebSocket> GetSessionWebSocket(MongoId sessionID)
    {
        lock (_socketsLock)
        {
            return _sockets.GetValueOrDefault(sessionID)?.Values.Where(s => s.State == WebSocketState.Open) ?? [];
        }
    }

    private static string GetSessionId(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var lastSlash = path.LastIndexOf('/');
        return lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
    }

    private void PruneClosedSockets(Dictionary<string, WebSocket> sessionSockets)
    {
        foreach (var (contextId, socket) in sessionSockets.Where(kvp => kvp.Value.State != WebSocketState.Open).ToArray())
        {
            sessionSockets.Remove(contextId);
            if (_sendGates.TryRemove(socket, out var gate))
            {
                gate.Dispose();
            }
        }
    }
}
