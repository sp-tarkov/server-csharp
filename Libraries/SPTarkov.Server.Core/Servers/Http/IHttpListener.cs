using Microsoft.AspNetCore.Http;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Servers.Http;

public interface IHttpListener
{
    bool CanHandle(MongoId sessionId, HttpContext context);
    Task Handle(MongoId sessionId, HttpContext context);
}
