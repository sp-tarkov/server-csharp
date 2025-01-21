using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Customization;
using Core.Models.Eft.ItemEvent;
using Core.Models.Utils;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class CustomizationItemEventRouter : ItemEventRouterDefinition
{
    protected ISptLogger<CustomizationItemEventRouter> _logger;
    protected CustomizationCallbacks _customizationCallbacks;

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
        return new()
        {
            new HandledRoute("CustomizationBuy", false),
            new HandledRoute("CustomizationSet", false)
        };
    }

    public override Task<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "CustomizationBuy":
                return Task.FromResult(_customizationCallbacks.BuyCustomisation(pmcData, body as BuyClothingRequestData, sessionID));
            case "CustomizationSet":
                return Task.FromResult(_customizationCallbacks.SetClothing(pmcData, body as CustomizationSetRequest, sessionID));
            default:
                throw new Exception($"CustomizationItemEventRouter being used when it cant handle route {url}");
        }
    }
}
