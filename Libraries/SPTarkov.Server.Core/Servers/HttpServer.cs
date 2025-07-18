using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer(
    ISptLogger<HttpServer> _logger,
    ServerLocalisationService _serverLocalisationService,
    ConfigServer _configServer,
    WebSocketServer _webSocketServer,
    ProfileActivityService _profileActivityService,
    IEnumerable<IHttpListener> _httpListeners
)
{
    private readonly HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();

    public async Task HandleRequest(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _webSocketServer.OnConnection(context);
            return;
        }

        context.Request.Cookies.TryGetValue("PHPSESSID", out var sessionId);
        if (sessionId != null)
        {
            _profileActivityService.SetActivityTimestamp(sessionId);
        }

        // Extract header for original IP detection
        var realIp = context.GetHeaderIfExists("x-real-ip");
        var clientIp = GetClientIp(context, realIp);

        if (_httpConfig.LogRequests)
        {
            LogRequest(context, clientIp, IsLocalRequest(clientIp));
        }

        try
        {
            var listener = _httpListeners.FirstOrDefault(l =>
                l.CanHandle(sessionId, context.Request)
            );

            if (listener != null)
            {
                await listener.Handle(sessionId, context.Request, context.Response);
            }
        }
        catch (Exception ex)
        {
            _logger.Critical("Error handling request: " + context.Request.Path);
            _logger.Critical(ex.Message);
            _logger.Critical(ex.StackTrace);
#if DEBUG
            throw; // added this so we can debug something.
#endif
        }

        // This http request would be passed through the SPT Router and handled by an ICallback
    }

    /// <summary>
    ///     Log request - handle differently if request is local
    /// </summary>
    /// <param name="context">HttpContext of request</param>
    /// <param name="clientIp">Ip of requester</param>
    /// <param name="isLocalRequest">Is this local request</param>
    protected void LogRequest(HttpContext context, string clientIp, bool isLocalRequest)
    {
        if (isLocalRequest)
        {
            _logger.Info(
                _serverLocalisationService.GetText("client_request", context.Request.Path.Value)
            );
        }
        else
        {
            _logger.Info(
                _serverLocalisationService.GetText(
                    "client_request_ip",
                    new { ip = clientIp, url = context.Request.Path.Value }
                )
            );
        }
    }

    protected static string GetClientIp(HttpContext context, StringValues? realIp)
    {
        if (realIp.HasValue)
        {
            return realIp.Value.First();
        }

        var forwardedFor = context.GetHeaderIfExists("x-forwarded-for");
        return forwardedFor.HasValue
            ? forwardedFor.Value.First()!.Split(",")[0].Trim()
            : context.Connection.RemoteIpAddress!.ToString().Split(":").Last();
    }

    /// <summary>
    ///     Check against hardcoded values that determine it's from a local address
    /// </summary>
    /// <param name="remoteAddress"> Address to check </param>
    /// <returns> True if its local </returns>
    protected bool IsLocalRequest(string? remoteAddress)
    {
        if (remoteAddress == null)
        {
            return false;
        }

        return remoteAddress.StartsWith("127.0.0")
            || remoteAddress.StartsWith("192.168.")
            || remoteAddress.StartsWith("localhost");
    }

    protected Dictionary<string, string> GetCookies(HttpRequest req)
    {
        var found = new Dictionary<string, string>();

        foreach (var keyValuePair in req.Cookies)
        {
            found.Add(keyValuePair.Key, keyValuePair.Value);
        }

        return found;
    }

    public string ListeningUrl()
    {
        return $"https://{_httpConfig.Ip}:{_httpConfig.Port}";
    }
}
