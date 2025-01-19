using Core.Context;
using Core.Servers.Http;
using Core.Services;
using Microsoft.Extensions.Primitives;
using SptCommon.Annotations;
using Core.Models.Spt.Config;
using Core.Models.Utils;


namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer(
    ISptLogger<HttpServer> _logger,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    ApplicationContext _applicationContext,
    WebSocketServer _webSocketServer,
    IEnumerable<IHttpListener> _httpListeners
)
{
    private readonly HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();
    private bool started;

    public void Load(WebApplicationBuilder? builder)
    {
        builder?.WebHost.UseKestrel();
        //builder.Services.AddControllers();
        // At the end
        var app = builder?.Build();

        // enable web socket
        app?.UseWebSockets();

        app?.Use((HttpContext req, RequestDelegate _) => { return Task.Factory.StartNew(() => HandleFallback(req)); });
        started = true;
        if (app is null)
        {
            throw new Exception($"Application context is null in HttpServer.Load()");
        }

        _applicationContext.AddValue(ContextVariableType.WEB_APPLICATION, app);
    }

    private Task HandleFallback(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            return _webSocketServer.OnConnection(context);
        }
        else
        {
            context.Request.Cookies.TryGetValue("PHPSESSID", out var sessionId);
            _applicationContext.AddValue(ContextVariableType.SESSION_ID, sessionId);

            // Extract headers for original IP detection
            StringValues? realIp = null;
            if (context.Request.Headers.ContainsKey("x-real-ip"))
                realIp = context.Request.Headers["x-real-ip"];
            StringValues? forwardedFor = null;
            if (context.Request.Headers.ContainsKey("x-forwarded-for"))
                forwardedFor = context.Request.Headers["x-forwarded-for"];

            var clientIp = realIp.HasValue
                ? realIp.Value.First()
                : forwardedFor.HasValue
                    ? forwardedFor.Value.First()!.Split(",")[0].Trim()
                    : context.Connection.RemoteIpAddress!.ToString();

            if (_httpConfig.LogRequests)
            {
                var isLocalRequest = IsLocalRequest(clientIp);
                if (isLocalRequest.HasValue)
                {
                    if (isLocalRequest.Value)
                        _logger.Info(_localisationService.GetText("client_request", context.Request.Path.Value));
                    else
                        _logger.Info(
                            _localisationService.GetText(
                                "client_request_ip",
                                new Dictionary<string, string>
                                    { { "ip", clientIp }, { "url", context.Request.Path.Value } }
                            )
                        );
                }
            }


            _httpListeners.SingleOrDefault(l => l.CanHandle(sessionId, context.Request))?.Handle(sessionId, context.Request, context.Response);
            // This http request would be passed through the SPT Router and handled by an ICallback
        }

        return Task.CompletedTask;
    }

    private bool? IsLocalRequest(string? remoteAddress)
    {
        if (remoteAddress == null) return null;

        return remoteAddress.StartsWith("127.0.0") ||
               remoteAddress.StartsWith("192.168.") ||
               remoteAddress.StartsWith("localhost");
    }

    protected Dictionary<string, string> GetCookies(HttpRequest req)
    {
        var found = new Dictionary<string, string>();

        foreach (var keyValuePair in req.Cookies) found.Add(keyValuePair.Key, keyValuePair.Value);

        return found;
    }

    public bool IsStarted()
    {
        return started;
    }
}
