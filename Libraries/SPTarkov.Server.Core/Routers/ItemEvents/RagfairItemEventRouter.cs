using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class RagfairItemEventRouter(RagfairCallbacks ragfairCallbacks) : ItemEventRouterDefinition
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.RAGFAIR_ADD_OFFER, false),
            new(ItemEventActions.RAGFAIR_REMOVE_OFFER, false),
            new(ItemEventActions.RAGFAIR_RENEW_OFFER, false),
        };
    }

    protected override ValueTask<ItemEventRouterResponse> HandleItemEventInternal(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        switch (url)
        {
            case ItemEventActions.RAGFAIR_ADD_OFFER:
                return new ValueTask<ItemEventRouterResponse>(ragfairCallbacks.AddOffer(pmcData, body as AddOfferRequestData, sessionID));
            case ItemEventActions.RAGFAIR_REMOVE_OFFER:
                return new ValueTask<ItemEventRouterResponse>(
                    ragfairCallbacks.RemoveOffer(pmcData, body as RemoveOfferRequestData, sessionID)
                );
            case ItemEventActions.RAGFAIR_RENEW_OFFER:
                return new ValueTask<ItemEventRouterResponse>(
                    ragfairCallbacks.ExtendOffer(pmcData, body as ExtendOfferRequestData, sessionID)
                );
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
