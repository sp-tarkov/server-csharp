using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class RepairItemEventRouter : ItemEventRouterDefinition
{
    protected RepairCallbacks _repairCallbacks;

    public RepairItemEventRouter
    (
        RepairCallbacks repairCallbacks
    )
    {
        _repairCallbacks = repairCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.REPAIR, false),
            new(ItemEventActions.TRADER_REPAIR, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.REPAIR:
                return _repairCallbacks.Repair(pmcData, body as RepairActionDataRequest, sessionID);
            case ItemEventActions.TRADER_REPAIR:
                return _repairCallbacks.TraderRepair(pmcData, body as TraderRepairActionDataRequest, sessionID);
            default:
                throw new Exception($"RepairItemEventRouter being used when it cant handle route {url}");
        }
    }
}
