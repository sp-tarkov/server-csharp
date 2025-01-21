using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
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
        return new()
        {
            new HandledRoute("RagFairAddOffer", false),
            new HandledRoute("RagFairRemoveOffer", false),
            new HandledRoute("RagFairRenewOffer", false),
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url) {
            case "RagFairAddOffer":
                return _ragfairCallbacks.AddOffer(pmcData, body as AddOfferRequestData, sessionID);
            case "RagFairRemoveOffer":
                return _ragfairCallbacks.RemoveOffer(pmcData, body as RemoveOfferRequestData, sessionID);
            case "RagFairRenewOffer":
                return _ragfairCallbacks.ExtendOffer(pmcData, body as ExtendOfferRequestData, sessionID);
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
