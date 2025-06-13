using System.Net.WebSockets;

namespace SPTarkov.Server.Core.Servers.Ws;

public interface IWebSocketConnectionHandler
{
    string GetHookUrl();
    string GetSocketId();
    Task OnConnection(WebSocket ws, HttpContext context, string sessionIdContext);
    Task OnMessage(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context);
    /// <summary>
    /// OnClose event of a WebSocket, it should already be assumed here that the WebSocket is closed.
    /// </summary>
    Task OnClose(WebSocket ws, HttpContext context, string sessionIdContext);
}
