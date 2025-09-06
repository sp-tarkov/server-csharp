using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class HttpDynamicRouter(ImageRouter imageRouter, JsonUtil jsonUtil)
    : DynamicRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(".jpg", async (_, _, _, _) => await imageRouter.GetImage()),
            new RouteAction<EmptyRequestData>(".png", async (_, _, _, _) => await imageRouter.GetImage()),
            new RouteAction<EmptyRequestData>(".ico", async (_, _, _, _) => await imageRouter.GetImage()),
        ]
    ) { }
