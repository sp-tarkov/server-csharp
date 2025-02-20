using System.Net;
using System.Security.Authentication;
using Core.Context;
using Core.Helpers;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers.Http;
using Core.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Primitives;
using SptCommon.Annotations;

namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer(
    ISptLogger<HttpServer> _logger,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    CertificateHelper _certificateHelper,
    ApplicationContext _applicationContext,
    WebSocketServer _webSocketServer,
    IEnumerable<IHttpListener> _httpListeners
)
{
    private readonly HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();
    private bool _started;

    public void Load(WebApplicationBuilder? builder)
    {
        if (builder is null)
        {
            throw new Exception("WebApplicationBuilder is null in HttpServer.Load()");
        }

        builder.WebHost.ConfigureKestrel(
            options =>
            {
                options.Listen(IPAddress.Parse(_httpConfig.Ip), _httpConfig.Port, listenOptions =>
                {
                    listenOptions.UseHttps(opts =>
                    {
                        opts.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                        opts.ServerCertificate = _certificateHelper.LoadOrGenerateCertificatePfx();
                        opts.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                    });
                });
            });

        var app = builder.Build();

        if (app is null)
        {
            throw new Exception("WebApplication is null in HttpServer.Load()");
        }

        // Enable web socket
        app.UseWebSockets(new WebSocketOptions
        {
            // Every minute a heartbeat is sent to keep the connection alive.
            KeepAliveInterval = TimeSpan.FromSeconds(60)
        });

        app.MapFallback(HandleFallback);

        _started = true;

        _applicationContext.AddValue(ContextVariableType.WEB_APPLICATION, app);
    }

    private async Task HandleFallback(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _webSocketServer.OnConnection(context);
            return;
        }

        context.Request.Cookies.TryGetValue("PHPSESSID", out var sessionId);
        _applicationContext.AddValue(ContextVariableType.SESSION_ID, sessionId);

        // Extract headers for original IP detection
        StringValues? realIp = null;
        if (context.Request.Headers.ContainsKey("x-real-ip"))
        {
            realIp = context.Request.Headers["x-real-ip"];
        }

        StringValues? forwardedFor = null;
        if (context.Request.Headers.ContainsKey("x-forwarded-for"))
        {
            forwardedFor = context.Request.Headers["x-forwarded-for"];
        }

        var clientIp = realIp.HasValue
            ? realIp.Value.First()
            : forwardedFor.HasValue
                ? forwardedFor.Value.First()!.Split(",")[0].Trim()
                : context.Connection.RemoteIpAddress!.ToString().Split(":").Last();

        if (_httpConfig.LogRequests)
        {
            var isLocalRequest = IsLocalRequest(clientIp);
            if (isLocalRequest.HasValue)
            {
                if (isLocalRequest.Value)
                {
                    _logger.Info(_localisationService.GetText("client_request", context.Request.Path.Value));
                }
                else
                {
                    _logger.Info(
                        _localisationService.GetText(
                            "client_request_ip", new
                            {
                                ip = clientIp,
                                url = context.Request.Path.Value
                            }
                        )
                    );
                }
            }
        }


        _httpListeners.SingleOrDefault(l => l.CanHandle(sessionId, context.Request))?.Handle(sessionId, context.Request, context.Response);
        // This http request would be passed through the SPT Router and handled by an ICallback
    }

    private bool? IsLocalRequest(string? remoteAddress)
    {
        if (remoteAddress == null)
        {
            return null;
        }

        return remoteAddress.StartsWith("127.0.0") ||
               remoteAddress.StartsWith("192.168.") ||
               remoteAddress.StartsWith("localhost");
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

    public bool IsStarted()
    {
        return _started;
    }

    public string ListeningUrl()
    {
        return $"https://{_httpConfig.Ip}:{_httpConfig.Port}";
    }
}
