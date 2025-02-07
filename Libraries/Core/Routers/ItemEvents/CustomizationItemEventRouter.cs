using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Customization;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class CustomizationItemEventRouter : ItemEventRouterDefinition
{
    protected CustomizationCallbacks _customizationCallbacks;
    protected ISptLogger<CustomizationItemEventRouter> _logger;

    public CustomizationItemEventRouter
    (
        ISptLogger<CustomizationItemEventRouter> logger,
        CustomizationCallbacks customizationCallbacks
    )
    {
        _logger = logger;
        _customizationCallbacks = customizationCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.CUSTOMIZATION_BUY, false),
            new(ItemEventActions.CUSTOMIZATION_SET, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.CUSTOMIZATION_BUY:
                return _customizationCallbacks.BuyCustomisation(pmcData, body as BuyClothingRequestData, sessionID);
            case ItemEventActions.CUSTOMIZATION_SET:
                return _customizationCallbacks.SetCustomisation(pmcData, body as CustomizationSetRequest, sessionID);
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
