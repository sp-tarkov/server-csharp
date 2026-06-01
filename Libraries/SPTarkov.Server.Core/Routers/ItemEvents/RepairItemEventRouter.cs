using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class RepairItemEventRouter(RepairCallbacks repairCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<RepairActionDataRequest>(
            ItemEventActions.REPAIR,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await repairCallbacks.Repair(pmcData, body, sessionID)
        ),
        new ItemRouteAction<TraderRepairActionDataRequest>(
            ItemEventActions.TRADER_REPAIR,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await repairCallbacks.TraderRepair(pmcData, body, sessionID)
        ),
    ]) { }
