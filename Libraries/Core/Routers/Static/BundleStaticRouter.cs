using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

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
                ) => bundleCallbacks.GetBundles(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
