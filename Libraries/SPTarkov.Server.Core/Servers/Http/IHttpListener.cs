namespace SPTarkov.Server.Core.Servers.Http;

public interface IHttpListener
{
    bool CanHandle(string sessionId, HttpRequest req);
    void Handle(string sessionId, HttpRequest req, HttpResponse resp);
}
