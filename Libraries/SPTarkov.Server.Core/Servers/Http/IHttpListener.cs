using Microsoft.AspNetCore.Http;

namespace SPTarkov.Server.Core.Servers.Http;

public interface IHttpListener
{
    bool CanHandle(string sessionId, HttpRequest req);
    Task Handle(string sessionId, HttpRequest req, HttpResponse resp);
}
