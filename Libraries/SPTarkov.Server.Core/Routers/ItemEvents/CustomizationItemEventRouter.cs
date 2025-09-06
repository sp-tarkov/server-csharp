using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Customization;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class CustomizationItemEventRouter(ISptLogger<CustomizationItemEventRouter> logger, CustomizationCallbacks customizationCallbacks)
    : ItemEventRouterDefinition
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new(ItemEventActions.CUSTOMIZATION_BUY, false), new(ItemEventActions.CUSTOMIZATION_SET, false)];
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
            case ItemEventActions.CUSTOMIZATION_BUY:
                return new ValueTask<ItemEventRouterResponse>(
                    customizationCallbacks.BuyCustomisation(pmcData, body as BuyClothingRequestData, sessionID)
                );
            case ItemEventActions.CUSTOMIZATION_SET:
                return new ValueTask<ItemEventRouterResponse>(
                    customizationCallbacks.SetCustomisation(pmcData, body as CustomizationSetRequest, sessionID)
                );
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
