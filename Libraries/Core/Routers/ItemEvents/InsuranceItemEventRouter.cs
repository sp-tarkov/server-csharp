using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Insurance;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using SptCommon.Annotations;

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
        return new List<HandledRoute>
        {
            new(ItemEventActions.INSURE, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.INSURE:
                return _insuranceCallbacks.Insure(pmcData, body as InsureRequestData, sessionID);
            default:
                throw new Exception($"InsuranceItemEventRouter being used when it cant handle route {url}");
        }
    }
}
