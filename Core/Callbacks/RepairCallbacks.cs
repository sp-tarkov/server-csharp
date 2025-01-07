using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Repair;

namespace Core.Callbacks;

public class RepairCallbacks
{
    public RepairCallbacks()
    {
        
    }
    
    public ItemEventRouterResponse TraderRepair(PmcData pmcData, TraderRepairActionDataRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse Repair(PmcData pmcData, RepairActionDataRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}