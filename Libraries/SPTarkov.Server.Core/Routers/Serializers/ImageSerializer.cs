using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace SPTarkov.Server.Core.Routers.Serializers;

[Injectable]
public class ImageSerializer : ISerializer
{
    protected ImageRouter _imageRouter;

    public ImageSerializer(ImageRouter imageRouter)
    {
        _imageRouter = imageRouter;
    }

    public void Serialize(string sessionID, HttpRequest req, HttpResponse resp, object? body)
    {
        _imageRouter.SendImage(sessionID, req, resp, body);
    }

    public bool CanHandle(string route)
    {
        return route == "IMAGE";
    }
}
