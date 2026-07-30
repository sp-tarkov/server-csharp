using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class BundleStaticRouter(JsonUtil jsonUtil, BundleCallbacks bundleCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/singleplayer/bundles",
                async (url, info, sessionID, output, cancellationToken) => await bundleCallbacks.GetBundles(url, info, sessionID)
            ),
        ]
    ) { }
