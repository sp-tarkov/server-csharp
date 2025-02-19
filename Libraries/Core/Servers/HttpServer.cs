using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Core.Context;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers.Http;
using Core.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
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
        if (builder is null)
        {
            throw new Exception("WebApplicationBuilder is null in HttpServer.Load()");
        }
        builder.Services.AddHttpsRedirection(conf =>
        {
            conf.HttpsPort = _httpConfig.Port;
        });
        builder.WebHost.ConfigureKestrel(
            options =>
            {
                const string certFileName = "certificate.pfx";
                var certificate = LoadCertificate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, certFileName, _httpConfig.CertificatePassword));
                if (certificate == null)
                {
                    // Generate self-signed certificate
                    certificate = GenerateSelfSignedCertificate("localhost");
                    SaveCertificate(certificate, certFileName); // Save cert

                    _logger.Success($"Generated and stored self-signed certificate ({certFileName}) in {AppDomain.CurrentDomain.BaseDirectory}");
                }

                options.ListenAnyIP(_httpConfig.Port, listenOptions =>
                {
                    listenOptions.UseHttps(opts =>
                    {
                        opts.SslProtocols = SslProtocols.Tls12;
                        opts.AllowAnyClientCertificate();
                        opts.ServerCertificate = certificate;
                        opts.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                    });
                });
            });

        var app = builder.Build();
        app.UseHttpsRedirection();
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

    /// <summary>
    /// Get a certificate from provided path and return
    /// </summary>
    /// <param name="pfxPath">Path to pfx file</param>
    /// <param name="certPassword">Optional password for certificate</param>
    /// <returns>X509Certificate2</returns>
    private X509Certificate2? LoadCertificate(string pfxPath, string? certPassword = null)
    {
        if (File.Exists(pfxPath))
        {
            try
            {
                //TODO: use this
                //return X509CertificateLoader.LoadCertificateFromFile(pfxPath);
                return string.IsNullOrEmpty(certPassword)
                    ? new X509Certificate2(pfxPath)
                    : new X509Certificate2(pfxPath, certPassword);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading certificate from path: {pfxPath} error: {ex.Message}");

                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Generate and return a self-signed certificate
    /// </summary>
    /// <param name="subjectName">e.g. localhost</param>
    /// <returns>X509Certificate2</returns>
    private X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
    {
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        sanBuilder.AddIpAddress(new IPAddress(new byte[] { 127, 0, 0, 1 }));
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddDnsName(Environment.MachineName);

        var distinguishedName = new X500DistinguishedName($"CN={subjectName}");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(sanBuilder.Build());

        return request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
    }

    /// <summary>
    /// Save a certificate as a file to disk
    /// </summary>
    /// <param name="certificate">Certificate to save</param>
    /// <param name="pfxPath">Path to destination</param>
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
