using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await customizationCallbacks.GetTraderSuits(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
