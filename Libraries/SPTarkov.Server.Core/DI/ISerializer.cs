using Microsoft.AspNetCore.Http;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.DI;

public interface ISerializer
{
    public Task Serialize(MongoId sessionID, HttpRequest req, HttpResponse resp, object? body);
    public bool CanHandle(string route);
}
