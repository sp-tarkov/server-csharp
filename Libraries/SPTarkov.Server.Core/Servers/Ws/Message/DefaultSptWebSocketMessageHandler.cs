using System.Net.WebSockets;
using System.Text;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Servers.Ws.Message;

[Injectable]
public class DefaultSptWebSocketMessageHandler(
    ISptLogger<DefaultSptWebSocketMessageHandler> _logger
) : ISptWebSocketMessageHandler
{
    public async Task OnSptMessage(string sessionID, WebSocket client, byte[] rawData)
    {
        _logger.Debug($"[{sessionID}] SPT message received: {Encoding.UTF8.GetString(rawData)}");
    }
}
