using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

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
