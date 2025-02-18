using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Core.Context;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers.Http;
using Core.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Primitives;
using SptCommon.Annotations;

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
    private bool _started;

    public void Load(WebApplicationBuilder? builder)
    {
        builder?.WebHost.UseKestrel(
            options =>
            {
                var certFileName = "certificate.pfx";
                var certificate = LoadCertificate(Path.Combine(Directory.GetCurrentDirectory(), certFileName));
                if (certificate == null)
                {
                    // Generate self-signed certificate
                    certificate = GenerateSelfSignedCertificate("localhost");
                    SaveCertificate(certificate, certFileName); // Save cert

                    _logger.Success($"Generated and stored self-signed certificate ({certFileName}) in {Directory.GetCurrentDirectory()}");
                }

                options.ListenAnyIP(_httpConfig.Port, listenOptions =>
                {
                    listenOptions.UseHttps(certificate);
                });
            }
            );
        //builder.Services.AddControllers();
        // At the end
        var app = builder?.Build();

        // enable web socket
        app?.UseWebSockets(new WebSocketOptions
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
        if (app is null)
        {
            throw new Exception("Application context is null in HttpServer.Load()");
        }

        _applicationContext.AddValue(ContextVariableType.WEB_APPLICATION, app);
    }

    private X509Certificate2? LoadCertificate(string pfxPath)
    {
        if (File.Exists(pfxPath))
        {
            try
            {
                //TODO: use this
                //return X509CertificateLoader.LoadCertificateFromFile(pfxPath);
                return new X509Certificate2(pfxPath);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading certificate from path: {pfxPath} error: {ex.Message}");

                return null;
            }
        }

        return null;
    }

    private X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
    {
        using var ecdsa = ECDsa.Create();
        var request = new CertificateRequest($"CN={subjectName}", ecdsa, HashAlgorithmName.SHA256);

        return request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
    }

    private void SaveCertificate(X509Certificate2 certificate, string pfxPath)
    {
        try
        {
            File.WriteAllBytes(pfxPath, certificate.Export(X509ContentType.Pfx));
        }
        catch (Exception ex)
        {
            _logger.Error($"Error saving certificate: {ex.Message}");
        }
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
                : context.Connection.RemoteIpAddress!.ToString();

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
                            "client_request_ip",
                            new Dictionary<string, string>
                            {
                                { "ip", clientIp },
                                { "url", context.Request.Path.Value }
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
