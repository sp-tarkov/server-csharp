using System.Net;
using System.Security.Authentication;
using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Primitives;
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;

namespace SPTarkov.Server.Core.Servers;

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

    /// <summary>
    /// Handle server loading event
    /// </summary>
    /// <param name="builder"> Server builder </param>
    /// <exception cref="Exception"> Throws Exception when WebApplicationBuiler or WebApplication are null </exception>
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

        app?.Use(
            (HttpContext req, RequestDelegate _) =>
            {
                return Task.Factory.StartNew(async () => await HandleFallback(req));
            }
        );

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
        if (sessionId != null)
        {
            _applicationContext.AddValue(ContextVariableType.SESSION_ID, sessionId);
        }

        // Extract header for original IP detection
        var realIp = context.GetHeaderIfExists("x-real-ip");
        var clientIp = GetClientIp(context, realIp);

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


        try
        {
            _httpListeners.SingleOrDefault(l => l.CanHandle(sessionId, context.Request))?.Handle(sessionId, context.Request, context.Response);
        }
        catch (Exception ex)
        {
            _logger.Critical(ex.Message);
            _logger.Critical(ex.StackTrace);
        }

        // This http request would be passed through the SPT Router and handled by an ICallback
    }

    protected static string? GetClientIp(HttpContext context, StringValues? realIp)
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
    /// Check against hardcoded values that determine it's from a local address
    /// </summary>
    /// <param name="remoteAddress"> Address to check </param>
    /// <returns> True if its local </returns>
    protected bool? IsLocalRequest(string? remoteAddress)
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
