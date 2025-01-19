using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Insurance;
using Core.Models.Eft.ItemEvent;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class InsuranceItemEventRouter : ItemEventRouterDefinition
{
    protected InsuranceCallbacks _insuranceCallbacks;

    public InsuranceItemEventRouter
    (
        InsuranceCallbacks insuranceCallbacks
    )
    {
        _insuranceCallbacks = insuranceCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new()
        {
            new HandledRoute("Insure", false)
        };
    }

    public override Task<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, object body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "Insure":
                return Task.FromResult(_insuranceCallbacks.Insure(pmcData, body as InsureRequestData, sessionID));
            default:
                throw new Exception($"InsuranceItemEventRouter being used when it cant handle route {url}");
        }
    }
}
