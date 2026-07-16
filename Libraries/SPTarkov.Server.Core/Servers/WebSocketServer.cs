using System.Buffers;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers.Ws;
using Microsoft.Extensions.Logging;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public sealed class WebSocketServer(IEnumerable<IWebSocketConnectionHandler> webSocketConnectionHandler, ISptLogger<WebSocketServer> logger)
{
    private const int MaxMessageBytes = 4 * 1024 * 1024;

    public bool CanHandle(HttpContext context)
    {
        if (context.Request.Path.Value is null)
        {
            return false;
        }

        return webSocketConnectionHandler.Any(wsh => context.Request.Path.Value.Contains(wsh.GetHookUrl()));
    }

    public async Task OnConnectionAsync(HttpContext httpContext)
    {
        var socket = await httpContext.WebSockets.AcceptWebSocketAsync(
            new WebSocketAcceptContext { KeepAliveInterval = TimeSpan.FromSeconds(60), KeepAliveTimeout = TimeSpan.FromSeconds(15) }
        );

        await HandleWebSocketAsync(httpContext, socket);
    }

    private async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
    {
        var socketHandlers = webSocketConnectionHandler.Where(wsh => context.Request.Path.Value!.Contains(wsh.GetHookUrl())).ToArray();

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        var wsToken = cancellationTokenSource.Token;
        var webSocketIdContext = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Notifying handlers of new websocket connection opening with reference {webSocketIdContext}");
        }

        foreach (var wsh in socketHandlers)
        {
            await wsh.OnConnectionAsync(webSocket, context, webSocketIdContext);
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"[WS] Starting read loop for websocket reference {webSocketIdContext}");
        }

        var receiveBuffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
        var messageBuffer = new ArrayBufferWriter<byte>();

        try
        {
            while (!wsToken.IsCancellationRequested)
            {
                ValueWebSocketReceiveResult result;

                try
                {
                    result = await webSocket.ReceiveAsync(receiveBuffer.AsMemory(), wsToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (WebSocketException)
                {
                    break;
                }

                // Handle graceful close of the WebSocket
                // WebsocketSharp requires this as when Close() is called it will send a message to the WS server that it's about to close.
                // If this is not handled an exception is thrown on the client
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.Debug($"[WS] Reference {webSocketIdContext} sent close frame, stopping.");
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing..", CancellationToken.None);
                    break;
                }

                messageBuffer.Write(receiveBuffer.AsSpan(0, result.Count));

                if (messageBuffer.WrittenCount > MaxMessageBytes)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, "Message too large", CancellationToken.None);
                    break;
                }

                if (!result.EndOfMessage)
                {
                    continue;
                }

                var message = messageBuffer.WrittenSpan.ToArray();
                messageBuffer.Clear();

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        $"[WS] Read loop for websocket reference {webSocketIdContext} received new message. Notifying socket handlers."
                    );
                }

                foreach (var wsh in socketHandlers)
                {
                    await wsh.OnMessageAsync(message, result.MessageType, webSocket, context);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(receiveBuffer);

            // Disconnect has been received, cancel the token and send OnClose to the relevant WebSockets.
            await cancellationTokenSource.CancelAsync();

            foreach (var wsh in socketHandlers)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"[WS] OnClose for websocket reference {webSocketIdContext} requested");
                }

                await wsh.OnCloseAsync(webSocket, context, webSocketIdContext);
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"[WS] Websocket reference {webSocketIdContext} fully closed.");
            }
        }
    }
}
