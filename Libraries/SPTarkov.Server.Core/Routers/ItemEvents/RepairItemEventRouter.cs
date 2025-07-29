using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class RepairItemEventRouter(RepairCallbacks repairCallbacks) : ItemEventRouterDefinition
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute> { new(ItemEventActions.REPAIR, false), new(ItemEventActions.TRADER_REPAIR, false) };
    }

    public override ValueTask<ItemEventRouterResponse> HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        switch (url)
        {
            case ItemEventActions.REPAIR:
                return new ValueTask<ItemEventRouterResponse>(repairCallbacks.Repair(pmcData, body as RepairActionDataRequest, sessionID));
            case ItemEventActions.TRADER_REPAIR:
                return new ValueTask<ItemEventRouterResponse>(
                    repairCallbacks.TraderRepair(pmcData, body as TraderRepairActionDataRequest, sessionID)
                );
            default:
                throw new Exception($"RepairItemEventRouter being used when it cant handle route {url}");
        }
    }
}
