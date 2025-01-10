using Core.Annotations;
using Core.DI;
using Core.Utils;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class HttpDynamicRouter : DynamicRouter
{
    public HttpDynamicRouter(ImageRouter imageRouter, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new(".jpg", (_, _, _, _) => imageRouter.GetImage()),
            new(".png", (_, _, _, _) => imageRouter.GetImage()),
            new(".ico", (_, _, _, _) => imageRouter.GetImage())
        ]
    )
    {
    }
}
