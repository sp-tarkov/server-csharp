using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Repair;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
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
            new("Repair", false),
            new("TraderRepair", false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "Repair":
                return _repairCallbacks.Repair(pmcData, body as RepairActionDataRequest, sessionID);
            case "TraderRepair":
                return _repairCallbacks.TraderRepair(pmcData, body as TraderRepairActionDataRequest, sessionID);
            default:
                throw new Exception($"RepairItemEventRouter being used when it cant handle route {url}");
        }
    }
}
