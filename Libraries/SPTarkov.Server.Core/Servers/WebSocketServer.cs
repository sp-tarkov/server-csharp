using System;
using System.Net.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class WebSocketServer(
    IEnumerable<IWebSocketConnectionHandler> _webSocketConnectionHandler,
    ISptLogger<WebSocketServer> _logger
)
{
    public async Task OnConnection(HttpContext httpContext)
    {
        var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocket(httpContext, socket);
    }

    private async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
    {
        var socketHandlers = _webSocketConnectionHandler
            .Where(wsh => context.Request.Path.Value.Contains(wsh.GetHookUrl()))
            .ToList();

        var cts = new CancellationTokenSource();
        var wsToken = cts.Token;

        if (socketHandlers.Count == 0)
        {
            var message = $"Socket connection received for url {context.Request.Path.Value}, but there is no websocket handler configured for it!";
            _logger.Debug(message);
            await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, message, CancellationToken.None);
            return;
        }

        var webSocketIdContext = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] Notifying handlers of new websocket connection opening with reference {webSocketIdContext}");
        }

        foreach (var wsh in socketHandlers)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"WebSocketHandler \"{wsh.GetSocketId()}\" connected");
                }
            }

            await wsh.OnConnection(webSocket, context, webSocketIdContext);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] Starting read loop for websocket reference {webSocketIdContext}");
        }

        var thread = Task.Factory.StartNew(async () =>
        {
            var messageBuffer = new List<byte>();
            var receiveBuffer = new byte[1024 * 4];
            var socketClosing = false;

            while (!wsToken.IsCancellationRequested && !socketClosing)
            {
                var segment = new ArraySegment<byte>(receiveBuffer);

                WebSocketReceiveResult? result = null;

                try
                {
                    result = await webSocket.ReceiveAsync(segment, wsToken);
                }
                catch (WebSocketException wsException)
                {
                    if (wsException.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely
                    || webSocket.State == WebSocketState.Aborted || webSocket.State == WebSocketState.Closed)
                    {
                        socketClosing = true;
                        break;
                    }
                }

                // Continue handling here, the WebSocket is not closed so we should be good despite being null here
                if (result == null)
                {
                    continue;
                }

                // Handle graceful close of the WebSocket
                // WebsocketSharp requires this as when Close() is called it will send a message to the WS server that it's about to close.
                // If this is not handled an exception is thrown on the client
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.Debug($"[WS] WebSocket reference {webSocketIdContext} sent close frame, stopping.");
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing..", wsToken);
                    socketClosing = true;
                    break;
                }

                messageBuffer.AddRange(segment.Take(result.Count));

                if (result.EndOfMessage)
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"[WS] Read loop for websocket reference {webSocketIdContext} received new message. Notifying socket handlers.");
                    }

                    var message = messageBuffer.ToArray();

                    foreach (var wsh in socketHandlers)
                    {
                        await wsh.OnMessage(message, WebSocketMessageType.Text, webSocket, context);
                    }

                    messageBuffer.Clear();
                }
            }
        }, wsToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        var counter = 0;
        while (webSocket.State == WebSocketState.Open)
        {
            if (counter == 30 && _logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[WS] Websocket keep alive for reference {webSocketIdContext}. Thread state {thread.Status}. Websocket state {webSocket.State}");
                counter = 0;
            }
            else
            {
                counter++;
            }

            // Keep this thread sleeping unless this status changes.
            Thread.Sleep(1000);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] State for websocket reference {webSocketIdContext} is now {webSocket.State}, closing");
        }

        // Disconnect has been received, cancel the token and send OnClose to the relevant WebSockets.
        foreach (var wsh in socketHandlers)
        {
            await cts.CancelAsync();

            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[WS] OnClose for websocket reference {webSocketIdContext} requested");
            }

            await wsh.OnClose(webSocket, context, webSocketIdContext);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] Websocket reference {webSocketIdContext} fully closed.");
        }
    }
}
