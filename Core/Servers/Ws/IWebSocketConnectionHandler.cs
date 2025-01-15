using System.Net.WebSockets;

namespace Core.Servers.Ws;

public interface IWebSocketConnectionHandler
{
    string GetHookUrl();
    string GetSocketId();
    Task OnConnection(WebSocket ws, HttpContext context);
}
