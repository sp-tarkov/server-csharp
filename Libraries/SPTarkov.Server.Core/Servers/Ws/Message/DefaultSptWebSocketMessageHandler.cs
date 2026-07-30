using System.Net.WebSockets;
using System.Text;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Servers.Ws.Message;

[Injectable]
public class DefaultSptWebSocketMessageHandler(ISptLogger<DefaultSptWebSocketMessageHandler> logger) : ISptWebSocketMessageHandler
{
    public Task OnSptMessageAsync(string sessionID, WebSocket client, byte[] rawData)
    {
        logger.Debug($"[{sessionID}] SPT message received: {Encoding.UTF8.GetString(rawData)}");
        return Task.CompletedTask;
    }
}
