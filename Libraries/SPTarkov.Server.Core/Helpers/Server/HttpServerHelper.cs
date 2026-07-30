using System.Collections.Frozen;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Helpers.Server;

[Injectable(InjectionType.Singleton)]
public class HttpServerHelper(HttpConfig httpConfig, IHttpContextAccessor httpContextAccessor)
{
    private const string AllInterfacesIpv4 = "0.0.0.0";

    protected readonly FrozenDictionary<string, string> Mime = new Dictionary<string, string>
    {
        { "css", "text/css" },
        { "bin", "application/octet-stream" },
        { "html", "text/html" },
        { "jpg", "image/jpeg" },
        { "js", "text/javascript" },
        { "json", "application/json" },
        { "png", "image/png" },
        { "svg", "image/svg+xml" },
        { "txt", "text/plain" },
    }.ToFrozenDictionary();

    public string? GetMimeText(string key)
    {
        return Mime.GetValueOrDefault(key);
    }

    public string GetBackendHost()
    {
        // Return the regular backend ip if we aren't using 0.0.0.0
        if (httpConfig.BackendIp != AllInterfacesIpv4)
        {
            return httpConfig.BackendIp;
        }

        // If we're on 0.0.0.0, use the requested host - throw if there isn't one.
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null || !request.Host.HasValue)
        {
            throw new InvalidOperationException(
                "BackendIp is configured as 0.0.0.0 but there is no active request to resolve the backend host from."
            );
        }

        return request.Host.Host;
    }

    public string BuildUrl()
    {
        var host = GetBackendHost();

        // Default https port, no need to append 443
        if (httpConfig.BackendPort == 443)
        {
            return host;
        }

        return $"{host}:{httpConfig.BackendPort}";
    }

    /// <summary>
    /// Prepend http to the url:port
    /// </summary>
    /// <returns>URI</returns>
    public string GetBackendUrl()
    {
        return $"https://{BuildUrl()}";
    }

    /// <summary>
    /// Get websocket url + port
    /// </summary>
    /// <returns>wss:// address</returns>
    public string GetWebsocketUrl()
    {
        return $"wss://{BuildUrl()}";
    }

    public void SendTextJson(HttpResponse resp, object output)
    {
        resp.Headers.Append("Content-Type", Mime["json"]);
        resp.StatusCode = 200;
        //  TODO: figure this one out
        // resp.writeHead(200, "OK",  {
        //     "Content-Type": this.mime.json
        // });
        // resp.end(output);
    }
}
