namespace Core.Servers.Http;

public interface IHttpListener
{
    bool CanHandle(string sessionId, object req);
    Task Handle(string sessionId, object req, object resp);
}