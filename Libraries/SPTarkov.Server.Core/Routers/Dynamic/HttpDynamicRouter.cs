using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class HttpDynamicRouter : DynamicRouter
{
    public HttpDynamicRouter(ImageRouter imageRouter, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(".jpg", (_, _, _, _) => imageRouter.GetImage()),
            new RouteAction(".png", (_, _, _, _) => imageRouter.GetImage()),
            new RouteAction(".ico", (_, _, _, _) => imageRouter.GetImage())
        ]
    )
    {
    }
}
