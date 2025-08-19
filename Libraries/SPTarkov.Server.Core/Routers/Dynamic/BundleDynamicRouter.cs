using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class BundleDynamicRouter(JsonUtil jsonUtil, BundleCallbacks bundleCallbacks)
    : DynamicRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/files/bundle",
                async (url, info, sessionID, output) => await bundleCallbacks.GetBundle(url, info, sessionID)
            ),
        ]
    ) { }
