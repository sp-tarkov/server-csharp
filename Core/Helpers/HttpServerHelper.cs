using System.Buffers;
using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;

namespace Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class HttpServerHelper
{
    protected HttpConfig _httpConfig;

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

    public HttpServerHelper(ConfigServer configServer) 
    {
        _httpConfig = configServer.GetConfig<HttpConfig>(ConfigTypes.HTTP);
    }

    public string GetMimeText(string key)
    {
        return mime[key];
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
        return $"http://{BuildUrl()}";
    }

    /** Get websocket url + port */
    public string GetWebsocketUrl() 
    {
        return $"ws://${BuildUrl()}";
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
