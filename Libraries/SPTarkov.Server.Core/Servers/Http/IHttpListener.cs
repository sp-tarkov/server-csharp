using Microsoft.AspNetCore.Http;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Servers.Http;

public interface IHttpListener
{
    bool CanHandle(HttpContext context);
    Task HandleAsync(MongoId sessionId, HttpContext context, CancellationToken cancellationToken = default);
}
