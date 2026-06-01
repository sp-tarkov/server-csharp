using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class RepairCallbacks(RepairController repairController)
{
    /// <summary>
    ///     Handle TraderRepair event
    ///     use trader to repair item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> TraderRepair(PmcData pmcData, TraderRepairActionDataRequest info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(repairController.TraderRepair(sessionID, info, pmcData));
    }

    /// <summary>
    ///     Handle Repair event
    ///     Use repair kit to repair item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> Repair(PmcData pmcData, RepairActionDataRequest info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(repairController.RepairWithKit(sessionID, info, pmcData));
    }
}
