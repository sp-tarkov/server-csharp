using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws.Message;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers.Ws;

[Injectable(InjectionType.Singleton)]
public class SptWebSocketConnectionHandler(
    ISptLogger<SptWebSocketConnectionHandler> logger,
    ServerLocalisationService serverLocalisationService,
    JsonUtil jsonUtil,
    ProfileHelper profileHelper,
    IEnumerable<ISptWebSocketMessageHandler> messageHandlers
) : IWebSocketConnectionHandler
{
    protected readonly Dictionary<MongoId, Dictionary<string, WebSocket>> _sockets = new();
    protected readonly Lock _socketsLock = new();

    public string GetHookUrl()
    {
        return "/notifierServer/getwebsocket/";
    }

    public string GetSocketId()
    {
        return "SPT WebSocket Handler";
    }

    public Task OnConnection(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = new MongoId(splitUrl.Last());
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
                if (sessionSockets.Any())
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
                sessionSockets = new Dictionary<string, WebSocket>();
                _sockets.Add(sessionID, sessionSockets);
            }

            sessionSockets.Add(sessionIdContext, ws);
            if (logger.IsLogEnabled(LogLevel.Info))
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

    public async Task OnMessage(byte[] receivedMessage, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Message for session {sessionID} received. Notifying message handlers.");
        }

        foreach (var sptWebSocketMessageHandler in messageHandlers)
        {
            await sptWebSocketMessageHandler.OnSptMessage(sessionID, ws, receivedMessage);
        }
    }

    public Task OnClose(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();

        lock (_socketsLock)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Attempting to close websocket session {sessionID} with context {sessionIdContext}");
            }

            if (_sockets.TryGetValue(sessionID, out var sessionSockets) && sessionSockets.Any())
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Websockets for session {sessionID} entry matched, attempting to find context {sessionIdContext}");
                }

                if (!sessionSockets.TryGetValue(sessionIdContext, out _) && logger.IsLogEnabled(LogLevel.Info))
                {
                    logger.Info(
                        $"[ws] The websocket session {sessionID} with reference: {sessionIdContext} has already been removed or reconnected"
                    );
                }
                else
                {
                    sessionSockets.Remove(sessionIdContext);
                    if (logger.IsLogEnabled(LogLevel.Info))
                    {
                        var playerProfile = profileHelper.GetFullProfile(sessionID);
                        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
                        logger.Info($"[ws] player: {playerInfoText} {sessionIdContext} has disconnected");
                    }
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

        return Task.CompletedTask;
    }

    public void SendMessageToAll(WsNotificationEvent output)
    {
        lock (_socketsLock)
        {
            foreach (var sessionID in _sockets.Keys)
            {
                SendMessage(sessionID, output); // this currently serializes for every socket, might want to separate into sending already serialized data
            }
        }
    }

    public void SendMessage(MongoId sessionID, WsNotificationEvent output)
    {
        try
        {
            if (IsWebSocketConnected(sessionID))
            {
                var webSockets = GetSessionWebSocket(sessionID);

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Send message for {sessionID} matched {webSockets.Count()} websockets. Messages being sent");
                }

                foreach (var webSocket in webSockets)
                {
                    var sendTask = webSocket.SendAsync(
                        Encoding.UTF8.GetBytes(jsonUtil.Serialize(output, output.GetType())),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Send message for {sessionID} on websocket async started");
                    }

                    sendTask.Wait();
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Send message for {sessionID} on websocket async finished");
                    }
                }

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(serverLocalisationService.GetText("websocket-message_sent"));
                }
            }
            else
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(serverLocalisationService.GetText("websocket-not_ready_message_not_sent", sessionID));
                }
            }
        }
        catch (Exception err)
        {
            logger.Error(serverLocalisationService.GetText("websocket-message_send_failed_with_error"), err);
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
}
