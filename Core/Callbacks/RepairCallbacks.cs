using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Repair;

namespace Core.Callbacks;

public class RepairCallbacks
{
    public RepairCallbacks()
    {
    }

    /// <summary>
    /// Handle TraderRepair event
    /// use trader to repair item
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse TraderRepair(PmcData pmcData, TraderRepairActionDataRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Repair event
    /// Use repair kit to repair item
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse Repair(PmcData pmcData, RepairActionDataRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}