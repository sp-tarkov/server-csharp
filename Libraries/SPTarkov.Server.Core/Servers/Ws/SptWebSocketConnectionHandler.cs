using System.Net.WebSockets;
using System.Text;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws.Message;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers.Ws;

[Injectable(InjectionType.Singleton)]
public class SptWebSocketConnectionHandler(
    ISptLogger<SptWebSocketConnectionHandler> _logger,
    ServerLocalisationService _serverLocalisationService,
    JsonUtil _jsonUtil,
    ProfileHelper _profileHelper,
    IEnumerable<ISptWebSocketMessageHandler> _messageHandlers
) : IWebSocketConnectionHandler
{
    protected readonly Dictionary<string, Dictionary<string, WebSocket>> _sockets = new();
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
        var sessionID = splitUrl.Last();
        var playerProfile = _profileHelper.GetFullProfile(sessionID);
        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"[WS] Websocket connect for player {playerInfoText} started with context {sessionIdContext}"
            );
        }

        lock (_socketsLock)
        {
            if (_sockets.TryGetValue(sessionID, out var sessionSockets))
            {
                if (sessionSockets.Any())
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug(
                            _serverLocalisationService.GetText(
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
            if (_logger.IsLogEnabled(LogLevel.Info))
            {
                _logger.Info(
                    _serverLocalisationService.GetText(
                        "websocket-player_connected",
                        new { sessionId = playerInfoText, contextId = sessionIdContext }
                    )
                );
            }

            return Task.CompletedTask;
        }
    }

    public async Task OnMessage(
        byte[] receivedMessage,
        WebSocketMessageType messageType,
        WebSocket ws,
        HttpContext context
    )
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"[WS] Message for session {sessionID} received. Notifying message handlers."
            );
        }

        foreach (var sptWebSocketMessageHandler in _messageHandlers)
        {
            await sptWebSocketMessageHandler.OnSptMessage(sessionID, ws, receivedMessage);
        }
    }

    public async Task OnClose(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();

        lock (_socketsLock)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Attempting to close websocket session {sessionID} with context {sessionIdContext}"
                );
            }

            if (_sockets.TryGetValue(sessionID, out var sessionSockets) && sessionSockets.Any())
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Websockets for session {sessionID} entry matched, attempting to find context {sessionIdContext}"
                    );
                }

                if (
                    !sessionSockets.TryGetValue(sessionIdContext, out _)
                    && _logger.IsLogEnabled(LogLevel.Info)
                )
                {
                    _logger.Info(
                        $"[ws] The websocket session {sessionID} with reference: {sessionIdContext} has already been removed or reconnected"
                    );
                }
                else
                {
                    sessionSockets.Remove(sessionIdContext);
                    if (_logger.IsLogEnabled(LogLevel.Info))
                    {
                        var playerProfile = _profileHelper.GetFullProfile(sessionID);
                        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
                        _logger.Info(
                            $"[ws] player: {playerInfoText} {sessionIdContext} has disconnected"
                        );
                    }
                }
            }
            else
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Websocket for session {sessionID} with context {sessionIdContext} does not exist on the socket map, nothing was removed"
                    );
                }
            }
        }
    }

    public void SendMessage(string sessionID, WsNotificationEvent output)
    {
        try
        {
            if (IsWebSocketConnected(sessionID))
            {
                var webSockets = GetSessionWebSocket(sessionID);

                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Send message for {sessionID} matched {webSockets.Count()} websockets. Messages being sent"
                    );
                }

                foreach (var webSocket in webSockets)
                {
                    var sendTask = webSocket.SendAsync(
                        Encoding.UTF8.GetBytes(_jsonUtil.Serialize(output, output.GetType())),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"Send message for {sessionID} on websocket async started");
                    }

                    sendTask.Wait();
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"Send message for {sessionID} on websocket async finished");
                    }
                }

                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(_serverLocalisationService.GetText("websocket-message_sent"));
                }
            }
            else
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        _serverLocalisationService.GetText(
                            "websocket-not_ready_message_not_sent",
                            sessionID
                        )
                    );
                }
            }
        }
        catch (Exception err)
        {
            _logger.Error(
                _serverLocalisationService.GetText("websocket-message_send_failed_with_error"),
                err
            );
        }
    }

    public bool IsWebSocketConnected(string sessionID)
    {
        lock (_socketsLock)
        {
            return _sockets.TryGetValue(sessionID, out var sockets)
                && sockets.Any(s => s.Value.State == WebSocketState.Open);
        }
    }

    public IEnumerable<WebSocket> GetSessionWebSocket(string sessionID)
    {
        lock (_socketsLock)
        {
            return _sockets
                    .GetValueOrDefault(sessionID)
                    ?.Values.Where(s => s.State == WebSocketState.Open) ?? [];
        }
    }
}
