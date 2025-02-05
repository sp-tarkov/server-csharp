using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class CustomizationStaticRouter : StaticRouter
{
    protected static CustomizationCallbacks _customizationCallbacks;

    public CustomizationStaticRouter(
        JsonUtil jsonUtil,
        CustomizationCallbacks customizationCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/trading/customization/storage",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _customizationCallbacks.GetCustomisationUnlocks(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/customization/offer/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _customizationCallbacks.GetHideoutCustomisation(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/customization/storage",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _customizationCallbacks.GetStorage(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
        _customizationCallbacks = customizationCallbacks;
    }
}
