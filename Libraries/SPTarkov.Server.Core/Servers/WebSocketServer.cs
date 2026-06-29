using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class WebSocketServer(IEnumerable<IWebSocketConnectionHandler> webSocketConnectionHandler, ISptLogger<WebSocketServer> logger)
{
    public bool CanHandle(HttpContext context)
    {
        return webSocketConnectionHandler.Any(wsh => context.Request.Path.Value.Contains(wsh.GetHookUrl()));
    }

    public async Task OnConnection(HttpContext httpContext)
    {
        var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocket(httpContext, socket);
    }

    private async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
    {
        var socketHandlers = webSocketConnectionHandler.Where(wsh => context.Request.Path.Value.Contains(wsh.GetHookUrl()));

        using var cts = new CancellationTokenSource();
        var wsToken = cts.Token;
        var webSocketIdContext = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Notifying handlers of new websocket connection opening with reference {webSocketIdContext}");
        }

        foreach (var wsh in socketHandlers)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"WebSocketHandler \"{wsh.GetSocketId()}\" connected");
                }
            }

            await wsh.OnConnection(webSocket, context, webSocketIdContext);
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Starting read loop for websocket reference {webSocketIdContext}");
        }

        try
        {
            var messageBuffer = new List<byte>();
            var receiveBuffer = new byte[1024 * 4];

            while (!wsToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                var segment = new ArraySegment<byte>(receiveBuffer);

                WebSocketReceiveResult? result = null;

                try
                {
                    result = await webSocket.ReceiveAsync(segment, wsToken);
                }
                catch (WebSocketException wsException)
                {
                    if (
                        wsException.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely
                        || webSocket.State == WebSocketState.Aborted
                        || webSocket.State == WebSocketState.Closed
                    )
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (result == null)
                {
                    continue;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.Debug($"[WS] WebSocket reference {webSocketIdContext} sent close frame, stopping.");
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing..", CancellationToken.None);
                    break;
                }

                messageBuffer.AddRange(segment.Take(result.Count));

                if (result.EndOfMessage)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            $"[WS] Read loop for websocket reference {webSocketIdContext} received new message. Notifying socket handlers."
                        );
                    }

                    var message = messageBuffer.ToArray();

                    foreach (var wsh in socketHandlers)
                    {
                        await wsh.OnMessage(message, WebSocketMessageType.Text, webSocket, context);
                    }

                    messageBuffer.Clear();
                }
            }
        }
        finally
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"[WS] State for websocket reference {webSocketIdContext} is now {webSocket.State}, closing");
            }

            await cts.CancelAsync();

            foreach (var wsh in socketHandlers)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"[WS] OnClose for websocket reference {webSocketIdContext} requested");
                }

                await wsh.OnClose(webSocket, context, webSocketIdContext);
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"[WS] Websocket reference {webSocketIdContext} fully closed.");
            }
        }
    }
}
