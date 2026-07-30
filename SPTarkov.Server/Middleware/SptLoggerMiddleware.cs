using System.Net;
using System.Net.Sockets;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Locales;

namespace SPTarkov.Server.Middleware;

public sealed class SptLoggerMiddleware(
    RequestDelegate next,
    ServerLocalisationService serverLocalisationService,
    HttpConfig httpConfig,
    ISptLogger<SptLoggerMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!httpConfig.LogRequests)
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

            if (ex.StackTrace is not null)
            {
                logger.Critical(ex.StackTrace);
            }
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
    public void LogRequest(HttpContext context, IPAddress clientIp, bool isLocalRequest, bool isWSRequest)
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
    private bool IsPrivateOrLocalAddress(IPAddress remoteAddress)
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
