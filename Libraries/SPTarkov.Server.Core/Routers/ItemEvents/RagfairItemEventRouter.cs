using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class RagfairItemEventRouter : ItemEventRouterDefinition
{
    protected RagfairCallbacks _ragfairCallbacks;

    public RagfairItemEventRouter
    (
        RagfairCallbacks ragfairCallbacks
    )
    {
        _ragfairCallbacks = ragfairCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.RAGFAIR_ADD_OFFER, false),
            new(ItemEventActions.RAGFAIR_REMOVE_OFFER, false),
            new(ItemEventActions.RAGFAIR_RENEW_OFFER, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.RAGFAIR_ADD_OFFER:
                return _ragfairCallbacks.AddOffer(pmcData, body as AddOfferRequestData, sessionID);
            case ItemEventActions.RAGFAIR_REMOVE_OFFER:
                return _ragfairCallbacks.RemoveOffer(pmcData, body as RemoveOfferRequestData, sessionID);
            case ItemEventActions.RAGFAIR_RENEW_OFFER:
                return _ragfairCallbacks.ExtendOffer(pmcData, body as ExtendOfferRequestData, sessionID);
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
