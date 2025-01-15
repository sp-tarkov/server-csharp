using System.Net.WebSockets;

namespace Core.Servers.Ws.Message;

public interface ISptWebSocketMessageHandler
{
    Task OnSptMessage(string sessionID, WebSocket client, byte[] rawData);
}
