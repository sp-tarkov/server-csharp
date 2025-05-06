using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class BundleStaticRouter : StaticRouter
{
    public BundleStaticRouter(
        JsonUtil jsonUtil,
        BundleCallbacks bundleCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/singleplayer/bundles",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => bundleCallbacks.GetBundles(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
