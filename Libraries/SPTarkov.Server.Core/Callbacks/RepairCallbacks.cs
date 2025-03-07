using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class RepairCallbacks(RepairController _repairController)
{
    /// <summary>
    ///     Handle TraderRepair event
    ///     use trader to repair item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse TraderRepair(PmcData pmcData, TraderRepairActionDataRequest info, string sessionID)
    {
        return _repairController.TraderRepair(sessionID, info, pmcData);
    }

    /// <summary>
    ///     Handle Repair event
    ///     Use repair kit to repair item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse Repair(PmcData pmcData, RepairActionDataRequest info, string sessionID)
    {
        return _repairController.RepairWithKit(sessionID, info, pmcData);
    }
}
