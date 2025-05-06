using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class HttpServerHelper(ConfigServer configServer)
{
    protected HttpConfig _httpConfig = configServer.GetConfig<HttpConfig>();

    protected Dictionary<string, string> mime = new()
    {
        { "css", "text/css" },
        { "bin", "application/octet-stream" },
        { "html", "text/html" },
        { "jpg", "image/jpeg" },
        { "js", "text/javascript" },
        { "json", "application/json" },
        { "png", "image/png" },
        { "svg", "image/svg+xml" },
        { "txt", "text/plain" }
    };

    public string? GetMimeText(string key)
    {
        return mime.GetValueOrDefault(key);
    }

    /**
     * Combine ip and port into address
     * @returns url
     */
    public string BuildUrl()
    {
        return $"{_httpConfig.BackendIp}:{_httpConfig.BackendPort}";
    }

    /**
     * Prepend http to the url:port
     * @returns URI
     */
    public string GetBackendUrl()
    {
        return $"https://{BuildUrl()}";
    }

    /**
     * Get websocket url + port
     */
    public string GetWebsocketUrl()
    {
        return $"wss://{BuildUrl()}";
    }

    public void SendTextJson(HttpResponse resp, object output)
    {
        resp.Headers.Append("Content-Type", mime["json"]);
        resp.StatusCode = 200;
        /* TODO: figure this one out
        resp.writeHead(200, "OK",  {
            "Content-Type": this.mime.json
        });
        resp.end(output);
        */
    }
}
