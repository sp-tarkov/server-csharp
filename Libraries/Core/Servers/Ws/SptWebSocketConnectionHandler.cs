using System.Net.WebSockets;
using System.Text;
using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Ws;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers.Ws.Message;
using Core.Services;
using Core.Utils;

namespace Core.Servers.Ws;

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
    protected HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();

    protected Dictionary<string, WebSocket> _sockets = new();
    protected Dictionary<string, Timer> _socketAliveTimers = new();
    protected Dictionary<string, CancellationTokenSource> _receiveTasks = new();
    protected Lock _lockObject = new();

    protected WsPing _defaultNotification = new();

    public string GetHookUrl() => "/notifierServer/getwebsocket/";
    public string GetSocketId() => "SPT WebSocket Handler";

    public Task OnConnection(WebSocket ws, HttpContext context)
    {
        return Task.Factory.StartNew(
            () =>
            {
                var splitUrl = context.Request.Path.Value.Split("/");
                var sessionID = splitUrl.Last();
                var playerProfile = _profileHelper.GetFullProfile(sessionID);
                var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";

                _logger.Info(_localisationService.GetText("websocket-player_connected", playerInfoText));

                _sockets.Add(sessionID, ws);

                lock (_lockObject)
                {
                    _receiveTasks.Add(sessionID, new());
                    var cancelToken = _receiveTasks[sessionID].Token;
                    Task.Factory.StartNew(_ => ReceiveTask(sessionID, ws, cancelToken), null, cancelToken);
                }

                while (ws.State == WebSocketState.Open)
                {
                    Thread.Sleep(1000);
                }

                // Once the websocket dies, we dispose of it
                //_logger.Debug(_localisationService.GetText("websocket-socket_lost_deleting_handle"));
                // this is expected and relayed via "Player has disconnected" i dont think this is needed
                lock (_lockObject)
                {
                    if (_socketAliveTimers.TryGetValue(sessionID, out var timer))
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _socketAliveTimers.Remove(sessionID);
                    }
                    if (_sockets.ContainsKey(sessionID))
                        _sockets.Remove(sessionID);
                    if (_receiveTasks.TryGetValue(sessionID, out var receiveTask))
                        receiveTask.CancelAsync().Wait();
                }
            }
        );
    }

    private void TimedTask(WebSocket ws, string sessionID)
    {
        _logger.Debug(_localisationService.GetText("websocket-pinging_player", sessionID));

        if (ws.State == WebSocketState.Open)
        {
            var sendTask = ws.SendAsync(
                Encoding.UTF8.GetBytes(_jsonUtil.Serialize(_defaultNotification)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            sendTask.Wait();
        }
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
                _logger.Debug(_localisationService.GetText("websocket-message_sent"));
            }
            else
            {
                _logger.Debug(_localisationService.GetText("websocket-not_ready_message_not_sent", sessionID));
            }
        }
        catch (Exception err)
        {
            _logger.Error(_localisationService.GetText("websocket-message_send_failed_with_error", err));
        }
    }

    private void ReceiveTask(string sessionID, WebSocket ws, CancellationToken cancelToken)
    {
        List<byte> readBytes = new();
        while (ws.State == WebSocketState.Open)
        {
            try
            {
                if (cancelToken.IsCancellationRequested)
                    break;
                var isEndOfMessage = false;
                while (!isEndOfMessage)
                {
                    var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
                    var readTask = ws.ReceiveAsync(buffer, cancelToken);
                    readTask.Wait(cancelToken);
                    readBytes.AddRange(buffer);
                    isEndOfMessage = readTask.Result.EndOfMessage;
                }

                foreach (var sptWebSocketMessageHandler in _messageHandlers)
                {
                    sptWebSocketMessageHandler.OnSptMessage(sessionID, ws, readBytes.ToArray()).Wait();
                }
            }
            catch (OperationCanceledException _)
            {
                _logger.Info("WebSocket disconnecting, receive task finalized...");
            }
            catch(Exception _)
            {
                lock (_lockObject)
                {
                    _sockets.Remove(sessionID);
                    _socketAliveTimers.Remove(sessionID);
                    _receiveTasks.Remove(sessionID);
                    var playerProfile = _profileHelper.GetFullProfile(sessionID);
                    var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
                    _logger.Info($"[ws] player: {playerInfoText} has disconnected");
                }
                
                ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed connection", CancellationToken.None);
            }
            finally
            {
                readBytes.Clear();
            }
        }
    }

    public bool IsWebSocketConnected(string sessionID)
    {
        return _sockets.TryGetValue(sessionID, out var socket) && socket.State == WebSocketState.Open;
    }

    public WebSocket GetSessionWebSocket(string sessionID)
    {
        return _sockets[sessionID];
    }
}
