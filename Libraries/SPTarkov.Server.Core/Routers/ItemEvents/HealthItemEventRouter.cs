using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class HealthItemEventRouter(HealthCallbacks healthCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<OffraidEatRequestData>(
            ItemEventActions.EAT,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await healthCallbacks.OffraidEat(pmcData, body, sessionID)
        ),
        new ItemRouteAction<OffraidHealRequestData>(
            ItemEventActions.HEAL,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await healthCallbacks.OffraidHeal(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HealthTreatmentRequestData>(
            ItemEventActions.RESTORE_HEALTH,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await healthCallbacks.HealthTreatment(pmcData, body, sessionID)
        ),
    ]) { }
