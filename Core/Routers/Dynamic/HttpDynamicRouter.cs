using Core.Annotations;
using Core.DI;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class HttpDynamicRouter : DynamicRouter
{
    public HttpDynamicRouter(ImageRouter imageRouter) : base(
        [
            new(".jpg", (_, _, _, _) => imageRouter.GetImage()),
            new(".png", (_, _, _, _) => imageRouter.GetImage()),
            new(".ico", (_, _, _, _) => imageRouter.GetImage())
        ]
    )
    {
    }

    public override Type? GetBodyDeserializationType()
    {
        return null;
    }
}
