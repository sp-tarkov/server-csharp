using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class BundleStaticRouter : StaticRouter
{
    protected static BundleCallbacks _bundleCallbacks;

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
                ) => _bundleCallbacks.GetBundles(url, info as EmptyRequestData, sessionID))
        ]
    )
    {
        _bundleCallbacks = bundleCallbacks;
    }
}
