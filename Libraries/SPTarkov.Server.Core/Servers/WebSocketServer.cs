using System.Net.WebSockets;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Common.Annotations;

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
            await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, message, CancellationToken.None);
            return;
        }

        foreach (var wsh in socketHandlers)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                _logger.Info($"WebSocketHandler \"{wsh.GetSocketId()}\" connected");
            }

            await wsh.OnConnection(webSocket, context);
        }

        // Discard this task, we dont need to await it.
        _ = Task.Factory.StartNew(async () =>
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

                foreach (var wsh in socketHandlers)
                {
                    await wsh.OnMessage(messageBuffer.ToArray(), WebSocketMessageType.Text, webSocket, context);
                }
            }
        }, TaskCreationOptions.LongRunning);

        while (webSocket.State == WebSocketState.Open)
        {
            // Keep this thread sleeping unless this status changes.
            Thread.Sleep(1000);
        }

        // Disconnect has been received, cancel the token and send OnClose to the relevant WebSockets.
        foreach (var wsh in socketHandlers)
        {
            await cts.CancelAsync();
            await wsh.OnClose(webSocket, context);
        }
    }
}
