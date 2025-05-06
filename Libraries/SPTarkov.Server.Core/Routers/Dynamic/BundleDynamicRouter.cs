using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
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
