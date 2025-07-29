using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Routers.Serializers;

[Injectable]
public class ImageSerializer(ImageRouter imageRouter) : ISerializer
{
    public async Task Serialize(MongoId sessionID, HttpRequest req, HttpResponse resp, object? body)
    {
        await imageRouter.SendImage(sessionID, req, resp, body);
    }

    public bool CanHandle(string route)
    {
        return route == "IMAGE";
    }
}
