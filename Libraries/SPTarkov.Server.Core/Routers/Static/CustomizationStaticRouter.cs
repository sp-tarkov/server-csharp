using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class CustomizationStaticRouter : StaticRouter
{
    public CustomizationStaticRouter(
        JsonUtil jsonUtil,
        CustomizationCallbacks customizationCallbacks
    )
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/trading/customization/storage",
                    (url, info, sessionID, output) =>
                        customizationCallbacks.GetCustomisationUnlocks(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        )
                ),
                new RouteAction(
                    "/client/hideout/customization/offer/list",
                    (url, info, sessionID, output) =>
                        customizationCallbacks.GetHideoutCustomisation(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        )
                ),
                new RouteAction(
                    "/client/customization/storage",
                    (url, info, sessionID, output) =>
                        customizationCallbacks.GetStorage(url, info as EmptyRequestData, sessionID)
                ),
            ]
        ) { }
}
