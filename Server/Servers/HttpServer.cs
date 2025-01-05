using System.Net.WebSockets;
using System.Text;
using Types.Annotations;
using Types.Context;
using Types.Models.Config;
using Types.Servers;
using Types.Services;
using ILogger = Types.Models.Utils.ILogger;

namespace Server.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer : IHttpServer
{
    protected HttpConfig httpConfig;
    protected bool started;

    private readonly ILogger _logger;
    private readonly ILocalisationService _localisationService;
    private readonly IConfigServer _configServer;
    private readonly IApplicationContext _applicationContext;
    private readonly IWebSocketServer _webSocketServer;

    public HttpServer(
        ILogger logger,
        ILocalisationService localisationService,
        IConfigServer configServer,
        IApplicationContext applicationContext,
        IWebSocketServer webSocketServer
    )
    {
        _logger = logger;
        _localisationService = localisationService;
        _configServer = configServer;
        _applicationContext = applicationContext;
        _webSocketServer = webSocketServer;

        // TODO: hook up the config server to put the HttpConfig here
        httpConfig = new HttpConfig() { Ip = "127.0.0.1", Port = 80 };
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
        app.UseEndpoints(endpointBuilder =>
        {
            endpointBuilder.MapFallback(HandleFallback);
        });
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
            // This http request would be passed through the SPT Router and handled by an ICallback
        }

        return Task.CompletedTask;
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