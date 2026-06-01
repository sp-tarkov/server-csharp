using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Customization;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class CustomizationItemEventRouter(CustomizationCallbacks customizationCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<BuyClothingRequestData>(
            ItemEventActions.CUSTOMIZATION_BUY,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(customizationCallbacks.BuyCustomisation(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<CustomizationSetRequest>(
            ItemEventActions.CUSTOMIZATION_SET,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(customizationCallbacks.SetCustomisation(pmcData, body, sessionID));
            }
        ),
    ]) { }
