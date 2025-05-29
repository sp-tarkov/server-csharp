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

        var message = $"Socket connection received for url {context.Request.Path.Value}, but there is no websocket handler configured for it!";
        _logger.Debug(message);
        if (socketHandlers.Count == 0)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, message, CancellationToken.None);
            return;
        }

        var sessionIdContext = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] Notifying handlers of new websocket connection openning with reference {sessionIdContext}");
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

            await wsh.OnConnection(webSocket, context, sessionIdContext);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] Starting read loop for websocket reference {sessionIdContext}");
        }
        // Discard this task, we dont need to await it.
        var thread = Task.Factory.StartNew(async () =>
        {
            while (!wsToken.IsCancellationRequested)
            {
                var messageBuffer = new byte[1024 * 4];
                var isEndOfMessage = false;

                while (!isEndOfMessage)
                {
                    var buffer = new ArraySegment<byte>(messageBuffer);
                    var readTask = await webSocket.ReceiveAsync(buffer, wsToken);
                    isEndOfMessage = readTask.EndOfMessage;
                }

                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[WS] Read loop for websocket reference {sessionIdContext} received new message. Notifying socket handlers.");
                }
                foreach (var wsh in socketHandlers)
                {
                    await wsh.OnMessage(messageBuffer.ToArray(), WebSocketMessageType.Text, webSocket, context);
                }
            }
        }, TaskCreationOptions.LongRunning);

        var counter = 0;
        while (webSocket.State == WebSocketState.Open)
        {
            if (counter == 30 && _logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[WS] Websocket keep alive for reference {sessionIdContext}. Thread state {thread.Status}. Websocket state {webSocket.State}");
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
            _logger.Debug($"[WS] State for websocket reference {sessionIdContext} is now {webSocket.State}, calling closing");
        }
        // Disconnect has been received, cancel the token and send OnClose to the relevant WebSockets.
        foreach (var wsh in socketHandlers)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[WS] Cancellation token for websocket reference {sessionIdContext} requested");
            }
            await cts.CancelAsync();
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[WS] OnClose for websocket reference {sessionIdContext} requested");
            }
            await wsh.OnClose(webSocket, context, sessionIdContext);
        }
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"[WS] Websocket reference {sessionIdContext} fully closed.");
        }
    }
}
