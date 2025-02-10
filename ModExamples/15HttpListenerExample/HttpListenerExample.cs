using System.Text;
using Core.Servers.Http;
using Microsoft.AspNetCore.Http;
using SptCommon.Annotations;

namespace _15HttpListenerExample;

[Injectable]
public class HttpListenerExample : IHttpListener
{
    public bool CanHandle(string sessionId, HttpRequest req)
    {
        return req.Method == "GET" && req.Method.Contains("/type-custom-url");
    }

    public void Handle(string sessionId, HttpRequest req, HttpResponse resp)
    {
        resp.StatusCode = 200;
        resp.Body.WriteAsync(Encoding.UTF8.GetBytes("[1] This is the first example of a mod hooking into the HttpServer")).AsTask().Wait();
        resp.
    }
}
