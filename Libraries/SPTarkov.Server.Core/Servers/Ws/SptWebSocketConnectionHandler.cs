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
    LocalisationService _localisationService,
    JsonUtil _jsonUtil,
    ProfileHelper _profileHelper,
    IEnumerable<ISptWebSocketMessageHandler> _messageHandlers
) : IWebSocketConnectionHandler
{
    protected Dictionary<string, Dictionary<string, WebSocket>> _sockets = new();
    protected Lock _socketsLock = new();

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
        lock (_socketsLock)
        {
            if (_sockets.TryGetValue(sessionID, out var sessionSockets) && sessionSockets.Any())
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(_localisationService.GetText("websocket-player_reconnect", playerInfoText));
                }

                foreach (var oldSocket in sessionSockets)
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"[ws] Removing websocket reference {oldSocket.Key} for session {sessionID}");
                    }

                    oldSocket.Value.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
                }

                sessionSockets.Clear();
            }
            else
            {
                sessionSockets = new Dictionary<string, WebSocket>();
                _sockets.Add(sessionID, sessionSockets);
            }

            sessionSockets.Add(sessionIdContext, ws);
            if (_logger.IsLogEnabled(LogLevel.Info))
            {
                _logger.Info(_localisationService.GetText("websocket-player_connected", playerInfoText));
            }

            return Task.CompletedTask;
        }
    }

    public async Task OnMessage(
        byte[] receivedMessage,
        WebSocketMessageType messageType,
        WebSocket ws,
        HttpContext context)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();

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
            if (_sockets.TryGetValue(sessionID, out var sessionSockets) && sessionSockets.Any())
            {
                if (!sessionSockets.TryGetValue(sessionIdContext, out _) && _logger.IsLogEnabled(LogLevel.Info))
                {
                    _logger.Info($"[ws] The websocket session {sessionID} with reference {sessionIdContext} has already been removed or reconnected");
                }
                else
                {
                    sessionSockets.Remove(sessionIdContext);
                    if (_logger.IsLogEnabled(LogLevel.Info))
                    {
                        var playerProfile = _profileHelper.GetFullProfile(sessionID);
                        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
                        _logger.Info($"[ws] player: {playerInfoText} has disconnected");
                    }
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

                foreach (var webSocket in webSockets)
                {
                    var sendTask = webSocket.SendAsync(
                        Encoding.UTF8.GetBytes(_jsonUtil.Serialize(output, output.GetType())),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                    sendTask.Wait();
                }

                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(_localisationService.GetText("websocket-message_sent"));
                }
            }
            else
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(_localisationService.GetText("websocket-not_ready_message_not_sent", sessionID));
                }
            }
        }
        catch (Exception err)
        {
            _logger.Error(_localisationService.GetText("websocket-message_send_failed_with_error"), err);
        }
    }

    public bool IsWebSocketConnected(string sessionID)
    {
        lock (_socketsLock)
        {
            return _sockets.TryGetValue(sessionID, out var sockets) && sockets.Any(s => s.Value.State == WebSocketState.Open);
        }
    }

    public IEnumerable<WebSocket> GetSessionWebSocket(string sessionID)
    {
        lock (_socketsLock)
        {
            return _sockets.GetValueOrDefault(sessionID)?.Values.Where(s => s.State == WebSocketState.Open);
        }
    }
}
