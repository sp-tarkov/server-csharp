using System.Net.WebSockets;
using System.Text;
using Core.Models.Utils;
using Core.Servers.Ws;
using SptCommon.Annotations;

namespace _24Websocket;
[Injectable(InjectionType = InjectionType.Singleton)]
public class CustomWebSocketConnectionHandler: IWebSocketConnectionHandler
{
    private readonly ISptLogger<CustomWebSocketConnectionHandler> _logger;
    public CustomWebSocketConnectionHandler(
        ISptLogger<CustomWebSocketConnectionHandler> logger
    )
    {
        _logger = logger;
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
        _logger.Info("Custom web socket is now connected!");

        return Task.CompletedTask;
    }

    public Task OnMessage(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        var msg = Encoding.UTF8.GetString(rawData);

        if (msg == "toodaloo")
        {
            return ws.SendAsync(Encoding.UTF8.GetBytes("toodaloo back!"), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        return Task.CompletedTask;
    }

    public Task OnClose(WebSocket ws, HttpContext context)
    {
        return Task.CompletedTask;
    }
}
