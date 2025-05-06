using System.Collections.Concurrent;
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
    ConfigServer _configServer,
    IEnumerable<ISptWebSocketMessageHandler> _messageHandlers
) : IWebSocketConnectionHandler
{
    protected WsPing _defaultNotification = new();
    protected ConcurrentDictionary<string, WebSocket> _sockets = new();

    public string GetHookUrl()
    {
        return "/notifierServer/getwebsocket/";
    }

    public string GetSocketId()
    {
        return "SPT WebSocket Handler";
    }

    public Task OnConnection(WebSocket ws, HttpContext context)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();
        var playerProfile = _profileHelper.GetFullProfile(sessionID);
        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";

        _logger.Info(_localisationService.GetText("websocket-player_connected", playerInfoText));

        if (!_sockets.TryAdd(sessionID, ws) && _logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[ws] player: {playerInfoText} has already connected");
        }

        return Task.CompletedTask;
    }

    public async Task OnMessage(byte[] receivedMessage, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();
        var playerProfile = _profileHelper.GetFullProfile(sessionID);
        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";

        foreach (var sptWebSocketMessageHandler in _messageHandlers)
        {
            await sptWebSocketMessageHandler.OnSptMessage(sessionID, ws, receivedMessage);
        }
    }

    public async Task OnClose(WebSocket ws, HttpContext context)
    {
        var splitUrl = context.Request.Path.Value.Split("/");
        var sessionID = splitUrl.Last();

        if (!_sockets.Remove(sessionID, out _) && _logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[ws] Error removing socket for session: {sessionID}");
        }

        var playerProfile = _profileHelper.GetFullProfile(sessionID);
        var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
        _logger.Info($"[ws] player: {playerInfoText} has disconnected");
    }

    public void SendMessage(string sessionID, WsNotificationEvent output)
    {
        try
        {
            if (IsWebSocketConnected(sessionID))
            {
                var ws = GetSessionWebSocket(sessionID);

                var sendTask = ws.SendAsync(
                    Encoding.UTF8.GetBytes(_jsonUtil.Serialize(output, output.GetType())),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
                sendTask.Wait();
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
        return _sockets.TryGetValue(sessionID, out var socket) && socket.State == WebSocketState.Open;
    }

    public WebSocket GetSessionWebSocket(string sessionID)
    {
        return _sockets.GetValueOrDefault(sessionID);
    }
}
