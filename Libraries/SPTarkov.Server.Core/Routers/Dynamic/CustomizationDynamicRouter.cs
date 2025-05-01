using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class CustomizationDynamicRouter : DynamicRouter
{
    public CustomizationDynamicRouter(
        JsonUtil jsonUtil,
        CustomizationCallbacks customizationCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/trading/customization/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => customizationCallbacks.GetTraderSuits(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
