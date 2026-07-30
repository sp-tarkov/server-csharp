using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Profile;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public sealed class HttpServer(
    HttpConfig httpConfig,
    WebSocketServer webSocketServer,
    ProfileActivityService profileActivityService,
    IEnumerable<IHttpListener> httpListeners
)
{
    public async Task HandleRequestAsync(HttpContext context, RequestDelegate next, CancellationToken cancellationToken = default)
    {
        if (context.WebSockets.IsWebSocketRequest && webSocketServer.CanHandle(context))
        {
            await webSocketServer.OnConnectionAsync(context);
            return;
        }

        var listener = httpListeners.FirstOrDefault(listener => listener.CanHandle(context));

        if (listener is null)
        {
            await next(context);
            return;
        }

        var sessionId = context.Request.Cookies.TryGetValue("PHPSESSID", out var sessionIdString)
            ? new MongoId(sessionIdString)
            : MongoId.Empty();

        if (!string.IsNullOrEmpty(sessionIdString))
        {
            profileActivityService.SetActivityTimestamp(sessionId);
        }

        await listener.HandleAsync(sessionId, context, cancellationToken);
    }

    public string ListeningUrl()
    {
        return $"https://{httpConfig.Ip}:{httpConfig.Port}";
    }
}
