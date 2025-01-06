using System.Net.WebSockets;
using Core.Context;
using Core.Models.Config;
using Core.Servers.Http;
using Core.Services;
using Microsoft.Extensions.Primitives;
using Core.Annotations;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer
{
    protected HttpConfig httpConfig;
    protected bool started;

    private readonly ILogger _logger;
    private readonly LocalisationService _localisationService;
    private readonly ConfigServer _configServer;
    private readonly ApplicationContext _applicationContext;
    private readonly WebSocketServer _webSocketServer;
    private readonly IEnumerable<IHttpListener> _httpListeners;

    public HttpServer(
        ILogger logger,
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

        // TODO: hook up the config server to put the HttpConfig here
        httpConfig = new HttpConfig() { Ip = "127.0.0.1", Port = 80, LogRequests = true};
    }

    public void Load(WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel();
        //builder.Services.AddControllers();
        // At the end
        var app = builder.Build();

        // enable web socket
        app.UseWebSockets();

        app.UseRouting();
        app.UseEndpoints(endpointBuilder => { endpointBuilder.MapFallback(HandleFallback); });
        started = true;
        app.Run($"http://{httpConfig.Ip}:{httpConfig.Port}");
    }

    private Task HandleFallback(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            return context.WebSockets.AcceptWebSocketAsync()
                .ContinueWith(task => Converse(task.Result));
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
                if (isLocalRequest.HasValue) {
                    if (isLocalRequest.Value) {
                        _logger.Info(_localisationService.GetText("client_request", req.url));
                    } else {
                        _logger.Info(
                            _localisationService.GetText("client_request_ip", {
                            ip: clientIp,
                            url: req.url.replaceAll("/", "\\"), // Localisation service escapes `/` into hex code `&#x2f;`
                        }),
                        );
                    }
                }
            }
            
            
            //_httpListeners.Single()
            // This http request would be passed through the SPT Router and handled by an ICallback
        }

        return Task.CompletedTask;
    }

    private bool? IsLocalRequest(string? remoteAddress)
    {
        if (remoteAddress == null) {
            return null;
        }

        return (
            remoteAddress.StartsWith("127.0.0") ||
            remoteAddress.StartsWith("192.168.") ||
            remoteAddress.StartsWith("localhost")
        );
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

    private void Converse(WebSocket connection)
    {
        var buffer = new byte[1024 * 4];
        var receive = connection.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        receive.Wait();
        var receiveResult = receive.Result;

        while (!receiveResult.CloseStatus.HasValue)
        {
            connection.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receive = connection.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            receive.Wait();
            receiveResult = receive.Result;
        }

        connection.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    public bool IsStarted() => started;
}