using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Customization;
using Core.Models.Eft.ItemEvent;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class CustomizationItemEventRouter : ItemEventRouterDefinition
{
    protected CustomizationCallbacks _customizationCallbacks;

    public CustomizationItemEventRouter
    (
        CustomizationCallbacks customizationCallbacks
    )
    {
        _customizationCallbacks = customizationCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new()
        {
            new HandledRoute("CustomizationBuy", false),
            new HandledRoute("CustomizationSet", false)
        };
    }

    public override object HandleItemEvent(string url, PmcData pmcData, object body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "CustomizationBuy":
                return _customizationCallbacks.BuyClothing(pmcData, body as BuyClothingRequestData, sessionID);
            case "CustomizationSet":
                return _customizationCallbacks.SetClothing(pmcData, body as CustomizationSetRequest, sessionID);
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
