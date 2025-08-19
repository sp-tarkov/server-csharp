using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class CustomizationStaticRouter(JsonUtil jsonUtil, CustomizationCallbacks customizationCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/trading/customization/storage",
                async (url, info, sessionID, output) => await customizationCallbacks.GetCustomisationUnlocks(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/hideout/customization/offer/list",
                async (url, info, sessionID, output) => await customizationCallbacks.GetHideoutCustomisation(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/customization/storage",
                async (url, info, sessionID, output) => await customizationCallbacks.GetStorage(url, info, sessionID)
            ),
        ]
    ) { }
