using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Health;
using Core.Models.Eft.ItemEvent;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class HealthItemEventRouter : ItemEventRouterDefinition
{
    protected HealthCallbacks _healthCallbacks;

    public HealthItemEventRouter
    (
        HealthCallbacks healthCallbacks
    )
    {
        _healthCallbacks = healthCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return
        [
            new HandledRoute("Eat", false),
            new HandledRoute("Heal", false),
            new HandledRoute("RestoreHealth", false)
        ];
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "Eat":
                return _healthCallbacks.OffraidEat(pmcData, body as OffraidEatRequestData, sessionID);
            case "Heal":
                return _healthCallbacks.OffraidHeal(pmcData, body as OffraidHealRequestData, sessionID);
            case "RestoreHealth":
                return _healthCallbacks.HealthTreatment(pmcData, body as HealthTreatmentRequestData, sessionID);
            default:
                throw new Exception($"HealthItemEventRouter being used when it cant handle route {url}");
        }
    }
}
