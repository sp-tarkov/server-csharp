using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class HttpDynamicRouter : DynamicRouter
{
    public HttpDynamicRouter(ImageRouter imageRouter, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(".jpg", async (_, _, _, _) => await imageRouter.GetImage()),
            new RouteAction(".png", async (_, _, _, _) => await imageRouter.GetImage()),
            new RouteAction(".ico", async (_, _, _, _) => await imageRouter.GetImage())
        ]
    )
    {
    }
}
