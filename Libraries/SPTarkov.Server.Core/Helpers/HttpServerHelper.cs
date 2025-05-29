using System.Net;
using System.Net.Sockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class HttpServerHelper(ConfigServer configServer)
{
    protected readonly HttpConfig _httpConfig = configServer.GetConfig<HttpConfig>();

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

    /// <summary>
    /// Combine ip and port into address
    /// </summary>
    /// <returns>URI</returns>
    public string BuildUrl()
    {
        return $"{_httpConfig.BackendIp}:{_httpConfig.BackendPort}";
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

    /// <summary>
    /// Method to determine if another version of the server is already running
    /// </summary>
    /// <returns>bool isAlreadyRunning</returns>
    public bool IsAlreadyRunning()
    {
        TcpListener? listener = null;

        try
        {
            listener = new(IPAddress.Parse(_httpConfig.Ip), _httpConfig.Port);
            listener.Start();
            return false;
        }
        catch (Exception)
        {
            return true;
        }
        finally
        {
            listener?.Stop();
        }
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
