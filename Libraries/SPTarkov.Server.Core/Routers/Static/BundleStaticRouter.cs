using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class BundleStaticRouter(JsonUtil jsonUtil, BundleCallbacks bundleCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/singleplayer/bundles",
                async (url, info, sessionID, output) => await bundleCallbacks.GetBundles(url, info, sessionID)
            ),
        ]
    ) { }
