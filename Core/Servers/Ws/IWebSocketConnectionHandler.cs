using System.Net.WebSockets;
using Core.Models.Eft.Ws;

namespace Core.Servers.Ws;

public interface IWebSocketConnectionHandler
{
    string GetHookUrl();
    string GetSocketId();
    Task OnConnection(WebSocket ws, HttpContext context);
    bool IsWebSocketConnected(string sessionId);

    Task SendMessageAsync(string sessionID, WsNotificationEvent output);
}
