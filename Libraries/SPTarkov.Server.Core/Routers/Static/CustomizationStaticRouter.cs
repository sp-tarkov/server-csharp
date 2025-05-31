using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class CustomizationStaticRouter : StaticRouter
{
    public CustomizationStaticRouter(
        JsonUtil jsonUtil,
        CustomizationCallbacks customizationCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/trading/customization/storage",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await customizationCallbacks.GetCustomisationUnlocks(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/customization/offer/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await customizationCallbacks.GetHideoutCustomisation(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/customization/storage",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await customizationCallbacks.GetStorage(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
