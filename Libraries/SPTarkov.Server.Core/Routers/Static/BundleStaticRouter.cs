using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
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
                ) => { return bundleCallbacks.GetBundles(url, info as EmptyRequestData, sessionID); })
        ]
    )
    {
    }
}
