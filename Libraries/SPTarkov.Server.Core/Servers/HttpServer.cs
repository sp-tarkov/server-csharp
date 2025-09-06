using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class HttpServer(
    ISptLogger<HttpServer> logger,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer,
    WebSocketServer webSocketServer,
    ProfileActivityService profileActivityService,
    IEnumerable<IHttpListener> httpListeners
)
{
    protected readonly HttpConfig HttpConfig = configServer.GetConfig<HttpConfig>();

    public async Task HandleRequest(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await webSocketServer.OnConnection(context);
            return;
        }

        // Use default empty mongoId if not found in cookie
        var sessionId = context.Request.Cookies.TryGetValue("PHPSESSID", out var sessionIdString)
            ? new MongoId(sessionIdString)
            : MongoId.Empty();
        if (!string.IsNullOrEmpty(sessionIdString))
        {
            profileActivityService.SetActivityTimestamp(sessionId);
        }

        var realIp = context.Connection.RemoteIpAddress ?? IPAddress.Parse("127.0.0.1");

        if (HttpConfig.LogRequests)
        {
            LogRequest(context, realIp, IsPrivateOrLocalAddress(realIp));
        }

        try
        {
            var listener = httpListeners.FirstOrDefault(listener => listener.CanHandle(sessionId, context.Request));

            if (listener != null)
            {
                await listener.Handle(sessionId, context.Request, context.Response);
            }
        }
        catch (Exception ex)
        {
            logger.Critical("Error handling request: " + context.Request.Path);
            logger.Critical(ex.Message);
            logger.Critical(ex.StackTrace);
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
    protected void LogRequest(HttpContext context, IPAddress clientIp, bool isLocalRequest)
    {
        if (isLocalRequest)
        {
            logger.Info(serverLocalisationService.GetText("client_request", context.Request.Path.Value));
        }
        else
        {
            logger.Info(serverLocalisationService.GetText("client_request_ip", new { ip = clientIp, url = context.Request.Path.Value }));
        }
    }

    /// <summary>
    ///     Check against hardcoded values that determine it's from a local address
    /// </summary>
    /// <param name="remoteAddress"> Address to check </param>
    /// <returns> True if its local </returns>
    protected bool IsPrivateOrLocalAddress(IPAddress remoteAddress)
    {
        if (IPAddress.IsLoopback(remoteAddress))
        {
            return true;
        }

        if (remoteAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = remoteAddress.GetAddressBytes();

            switch (bytes[0])
            {
                case 10:
                    return true; // 10.0.0.0/8 (private)

                case 169:
                    return bytes[1] == 254; // 169.254.0.0/16 (APIPA/link-local)

                case 172:
                    return bytes[1] >= 16 && bytes[1] <= 31; // 172.16.0.0/12 (private)

                case 192:
                    return bytes[1] == 168; // 192.168.0.0/16 (private)

                default:
                    return false;
            }
        }

        if (remoteAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (remoteAddress.IsIPv6LinkLocal)
            {
                return true;
            }
        }

        return false;
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
        return $"https://{HttpConfig.Ip}:{HttpConfig.Port}";
    }
}
