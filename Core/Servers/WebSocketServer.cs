using System.Net.WebSockets;
using System.Text;
using Core.Annotations;
using Core.Models.Utils;
using Core.Servers.Ws;
using Core.Utils;

namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class WebSocketServer
{
    protected IEnumerable<IWebSocketConnectionHandler> _webSocketConnectionHandler;
    protected ISptLogger<WebSocketServer> _logger;
    protected JsonUtil _jsonUtil;

    public WebSocketServer(
        IEnumerable<IWebSocketConnectionHandler> webSocketConnectionHandlers,
        ISptLogger<WebSocketServer> logger,
        JsonUtil jsonUtil
    )
    {
        _webSocketConnectionHandler = webSocketConnectionHandlers;
        _logger = logger;
        _jsonUtil = jsonUtil;
    }

    public async Task OnConnection(HttpContext httpContext)
    {
        var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
        await HandleCommunication(httpContext, socket);
    }

    private Task HandleCommunication(HttpContext context, WebSocket webSocket)
    {
        var socketHandlers = _webSocketConnectionHandler
            .Where(wsh => context.Request.Path.Value.Contains(wsh.GetHookUrl()))
            .ToList();
        if (socketHandlers.Count == 0)
        {
            var message = $"Socket connection received for url {context.Request.Path.Value}, but there is not websocket handler configured for it";
            _logger.Warning(message);
            return webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, message, CancellationToken.None);
        }

        foreach (var wsh in socketHandlers)
        {
            wsh.OnConnection(webSocket, context).Wait();
            _logger.Info($"WebSocketHandler \"{wsh.GetSocketId()}\" connected");
        }
        return Task.CompletedTask;
    }
}
