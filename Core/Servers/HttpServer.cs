using System.Net.WebSockets;
using Core.Context;
using Core.Servers.Http;
using Core.Services;
using Microsoft.Extensions.Primitives;
using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;


namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer
{
    protected HttpConfig httpConfig;
    protected bool started;

    protected ISptLogger<HttpServer> _logger;
    protected LocalisationService _localisationService;
    protected ConfigServer _configServer;
    protected ApplicationContext _applicationContext;
    protected WebSocketServer _webSocketServer;
    protected IEnumerable<IHttpListener> _httpListeners;

    public HttpServer(
        ISptLogger<HttpServer> logger,
        LocalisationService localisationService,
        ConfigServer configServer,
        ApplicationContext applicationContext,
        WebSocketServer webSocketServer,
        IEnumerable<IHttpListener> httpListeners
    )
    {
        _logger = logger;
        _localisationService = localisationService;
        _configServer = configServer;
        _applicationContext = applicationContext;
        _webSocketServer = webSocketServer;
        _httpListeners = httpListeners;

        httpConfig = _configServer.GetConfig<HttpConfig>();
    }

    public void Load(WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel();
        //builder.Services.AddControllers();
        // At the end
        var app = builder.Build();

        // enable web socket
        app.UseWebSockets();

        app.Use((HttpContext req, RequestDelegate _) =>
        {
            return Task.Factory.StartNew(() => HandleFallback(req));
        });
        started = true;
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

            if (httpConfig.LogRequests)
            {
                var isLocalRequest = IsLocalRequest(clientIp);
                if (isLocalRequest.HasValue)
                {
                    if (isLocalRequest.Value)
                        _logger.Info(_localisationService.GetText("client_request", context.Request.Path.Value));
                    else
                        _logger.Info(
                            _localisationService.GetText("client_request_ip",
                                new Dictionary<string, string>
                                    { { "ip", clientIp }, { "url", context.Request.Path.Value } })
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
