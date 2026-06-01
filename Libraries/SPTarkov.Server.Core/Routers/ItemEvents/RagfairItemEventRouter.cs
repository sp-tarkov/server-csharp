using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class RagfairItemEventRouter(RagfairCallbacks ragfairCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<AddOfferRequestData>(
            ItemEventActions.RAGFAIR_ADD_OFFER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(ragfairCallbacks.AddOffer(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<RemoveOfferRequestData>(
            ItemEventActions.RAGFAIR_REMOVE_OFFER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(ragfairCallbacks.RemoveOffer(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<ExtendOfferRequestData>(
            ItemEventActions.RAGFAIR_RENEW_OFFER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(ragfairCallbacks.ExtendOffer(pmcData, body, sessionID));
            }
        ),
    ]) { }
