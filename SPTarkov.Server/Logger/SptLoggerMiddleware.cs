using System.Net;
using System.Net.Sockets;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Logger;

public class SptLoggerMiddleware(
    RequestDelegate next,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer,
    ISptLogger<SptLoggerMiddleware> logger
)
{
    protected readonly HttpConfig HttpConfig = configServer.GetConfig<HttpConfig>();

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpConfig.LogRequests)
        {
            await next(context);
            return;
        }

        var realIp = context.Connection.RemoteIpAddress ?? IPAddress.Parse("127.0.0.1");
        LogRequest(context, realIp, IsPrivateOrLocalAddress(realIp), context.WebSockets.IsWebSocketRequest);

        try
        {
            await next(context);

            if (context.Response.StatusCode == 404)
            {
                logger.Error(serverLocalisationService.GetText("unhandled_response", context.Request.Path.ToString()));
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
    }

    /// <summary>
    ///     Log request - handle differently if request is local
    /// </summary>
    /// <param name="context">HttpContext of request</param>
    /// <param name="clientIp">Ip of requester</param>
    /// <param name="isLocalRequest">Is this local request</param>
    protected void LogRequest(HttpContext context, IPAddress clientIp, bool isLocalRequest, bool isWSRequest)
    {
        if (isWSRequest)
        {
            if (isLocalRequest)
            {
                logger.Info(serverLocalisationService.GetText("websocket_request", context.Request.Path.Value));
            }
            else
            {
                logger.Info(
                    serverLocalisationService.GetText("websocket_request_ip", new { ip = clientIp, url = context.Request.Path.Value })
                );
            }
        }
        else
        {
            if (isLocalRequest)
            {
                logger.Info(serverLocalisationService.GetText("client_request", context.Request.Path.Value));
            }
            else
            {
                logger.Info(
                    serverLocalisationService.GetText("client_request_ip", new { ip = clientIp, url = context.Request.Path.Value })
                );
            }
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
}
