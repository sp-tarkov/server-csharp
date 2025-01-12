using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class BundleDynamicRouter : DynamicRouter
{
    protected static BundleCallbacks _bundleCallbacks;

    public BundleDynamicRouter(
        JsonUtil jsonUtil,
        BundleCallbacks bundleCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/files/bundle",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _bundleCallbacks.GetBundle(url, info as EmptyRequestData, sessionID))
        ]
    )
    {
        _bundleCallbacks = bundleCallbacks;
    }
}
