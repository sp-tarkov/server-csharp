using System.Net.WebSockets;
using Core.Models.Utils;
using Core.Servers.Ws.Message;

namespace _24Websocket;

public class WebsocketMessageHandler : ISptWebSocketMessageHandler
{
    private readonly ISptLogger<WebsocketMessageHandler> _logger;

    public WebsocketMessageHandler(
        ISptLogger<WebsocketMessageHandler> logger
    )
    {
        _logger = logger;
    }

    public Task OnSptMessage(string sessionID, WebSocket client, byte[] rawData)
    {
        _logger.Info($"Custom SPT WebSocket Message handler received a message for {sessionID}: {rawData.ToString()}");
        return Task.CompletedTask;
    }
}
