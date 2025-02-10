using System.Net.WebSockets;
using System.Text;
using Core.Helpers;
using Core.Models.Eft.Ws;
using Core.Models.Utils;
using Core.Servers.Ws;
using Core.Servers.Ws.Message;
using Core.Utils;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace _24Websocket;
// TODO: this is basically a copy of what we do, what is NEEDED of each method and add comments
[Injectable(InjectionType = InjectionType.Singleton)]
public class WebsocketConnectionHandler : IWebSocketConnectionHandler
{
    private readonly ISptLogger<WebsocketConnectionHandler> _logger;
    private readonly ProfileHelper _profileHelper;
    private readonly JsonUtil _jsonUtil;
    private readonly IEnumerable<ISptWebSocketMessageHandler> _messageHandlers;

    protected WsPing _defaultNotification = new();
    protected Lock _lockObject = new();
    protected Dictionary<string, CancellationTokenSource> _receiveTasks = new();
    protected Dictionary<string, Timer> _socketAliveTimers = new();

    protected Dictionary<string, WebSocket> _sockets = new();

    public WebsocketConnectionHandler(
        ISptLogger<WebsocketConnectionHandler> logger,
        ProfileHelper profileHelper,
        JsonUtil jsonUtil,
        IEnumerable<ISptWebSocketMessageHandler> messageHandlers
    )
    {
        _logger = logger;
        _profileHelper = profileHelper;
        _jsonUtil = jsonUtil;
    }

    public string GetHookUrl()
    {
        return "/custom/socket/";
    }

    public string GetSocketId()
    {
        return "My Custom WebSocket";
    }

    public Task OnConnection(WebSocket ws, HttpContext context)
    {
        return Task.Factory.StartNew(
            () =>
            {
                var splitUrl = context.Request.Path.Value.Split("/");
                var sessionID = splitUrl.Last();
                var playerProfile = _profileHelper.GetFullProfile(sessionID);
                var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionID})";
                _logger.Info($"Custom web socket is now connected!: {playerInfoText}");

                _sockets.Add(sessionID, ws);

                lock (_lockObject)
                {
                    _receiveTasks.Add(sessionID, new CancellationTokenSource());
                    var cancelToken = _receiveTasks[sessionID].Token;
                    Task.Factory.StartNew(_ => ReceiveTask(sessionID, ws, cancelToken), null, cancelToken);
                }

                while (ws.State == WebSocketState.Open)
                {
                    Thread.Sleep(1000);
                }

                // Once the websocket dies, we dispose of it
                lock (_lockObject)
                {
                    if (_socketAliveTimers.TryGetValue(sessionID, out var timer))
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _socketAliveTimers.Remove(sessionID);
                    }

                    if (_sockets.ContainsKey(sessionID))
                    {
                        _sockets.Remove(sessionID);
                    }

                    if (_receiveTasks.TryGetValue(sessionID, out var receiveTask))
                    {
                        receiveTask.CancelAsync().Wait();
                    }
                }
            }
        );
    }

    private void ReceiveTask(string sessionId, WebSocket ws, CancellationToken cancelToken)
    {
        List<byte> readBytes = new();
        while (ws.State == WebSocketState.Open)
        {
            try
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

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
                    sptWebSocketMessageHandler.OnSptMessage(sessionId, ws, readBytes.ToArray()).Wait();
                }
            }
            catch (OperationCanceledException _)
            {
                _logger.Info("WebSocket disconnecting, receive task finalized...");
            }
            catch (Exception _)
            {
                lock (_lockObject)
                {
                    _sockets.Remove(sessionId);
                    _socketAliveTimers.Remove(sessionId);
                    _receiveTasks.Remove(sessionId);
                    var playerProfile = _profileHelper.GetFullProfile(sessionId);
                    var playerInfoText = $"{playerProfile.ProfileInfo.Username} ({sessionId})";
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

    public bool IsWebSocketConnected(string sessionId)
    {
        return _sockets.TryGetValue(sessionId, out var socket) && socket.State == WebSocketState.Open;
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
                    _logger.Debug("Sent Message");
                }
            }
            else
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug("Couldnt send Message");
                }
            }
        }
        catch (Exception err)
        {
            _logger.Error("message failed with error");
        }
    }

    public WebSocket GetSessionWebSocket(string sessionID)
    {
        return _sockets[sessionID];
    }
}
