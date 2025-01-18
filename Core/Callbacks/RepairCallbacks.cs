using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Repair;

namespace Core.Callbacks;

[Injectable]
public class RepairCallbacks(RepairController _repairController)
{
    /// <summary>
    /// Handle TraderRepair event
    /// use trader to repair item
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse TraderRepair(PmcData pmcData, TraderRepairActionDataRequest info, string sessionID)
    {
        return _repairController.TraderRepair(sessionID, info, pmcData);
    }

    /// <summary>
    /// Handle Repair event
    /// Use repair kit to repair item
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse Repair(PmcData pmcData, RepairActionDataRequest info, string sessionID)
    {
        return _repairController.RepairWithKit(sessionID, info, pmcData);
    }
}
