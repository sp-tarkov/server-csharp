using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

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
