using System.Net.WebSockets;
using System.Text;
using Core.Annotations;

namespace Core.Servers.Ws.Message;

[Injectable]
public class DefaultSptWebSocketMessageHandler(
    Models.Utils.ISptLogger<DefaultSptWebSocketMessageHandler> _logger
) : ISptWebSocketMessageHandler
{
    public async Task OnSptMessage(string sessionID, WebSocket client, byte[] rawData)
    {
        _logger.Debug($"[{sessionID}] SPT message received: {Encoding.UTF8.GetString(rawData)}");
    }
}
