using Microsoft.AspNetCore.Http;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.DI;

public interface ISerializer
{
    public Task SerializeAsync(
        MongoId sessionID,
        HttpRequest req,
        HttpResponse resp,
        object? body,
        CancellationToken cancellationToken = default
    );
    public bool CanHandle(string route);
}
