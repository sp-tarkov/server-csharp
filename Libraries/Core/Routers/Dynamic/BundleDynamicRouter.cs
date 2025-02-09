using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class BundleDynamicRouter : DynamicRouter
{
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
                ) => bundleCallbacks.GetBundle(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
