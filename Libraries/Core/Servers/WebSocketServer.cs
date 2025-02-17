using System.Net.WebSockets;
using Core.Models.Utils;
using Core.Servers.Ws;
using SptCommon.Annotations;

namespace Core.Servers;

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

        if (socketHandlers.Count == 0)
        {
            var message = $"Socket connection received for url {context.Request.Path.Value}, but there is not websocket handler configured for it";
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

        var messageBuffer = new byte[1024];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(messageBuffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text || receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    foreach (var wsh in socketHandlers)
                    {
                        await wsh.OnMessage(messageBuffer.ToArray(), receiveResult.MessageType, webSocket, context);
                    }
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    foreach (var wsh in socketHandlers)
                    {
                        await wsh.OnClose(webSocket, context);
                    }
                }
            }
        }
        catch (Exception)
        {
            foreach (var wsh in socketHandlers)
            {
                await wsh.OnClose(webSocket, context);
            }
        }
    }
}
