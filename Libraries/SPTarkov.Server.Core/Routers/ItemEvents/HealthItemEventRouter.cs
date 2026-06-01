using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class HealthItemEventRouter(HealthCallbacks healthCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<OffraidEatRequestData>(
            ItemEventActions.EAT,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(healthCallbacks.OffraidEat(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<OffraidHealRequestData>(
            ItemEventActions.HEAL,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(healthCallbacks.OffraidHeal(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HealthTreatmentRequestData>(
            ItemEventActions.RESTORE_HEALTH,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(healthCallbacks.HealthTreatment(pmcData, body, sessionID));
            }
        ),
    ]) { }
