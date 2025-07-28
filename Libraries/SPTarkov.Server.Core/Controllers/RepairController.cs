using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class RepairController(EventOutputHolder eventOutputHolder, RepairService repairService)
{
    /// <summary>
    ///     Handle TraderRepair event
    ///     Repair with trader
    /// </summary>
    /// <param name="sessionID">session id</param>
    /// <param name="request">endpoint request data</param>
    /// <param name="pmcData">player profile</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse TraderRepair(MongoId sessionID, TraderRepairActionDataRequest request, PmcData pmcData)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        // find the item to repair
        foreach (var repairItem in request.RepairItems)
        {
            var repairDetails = repairService.RepairItemByTrader(sessionID, pmcData, repairItem, request.TraderId);

            repairService.PayForRepair(sessionID, pmcData, repairItem.Id, repairDetails.RepairCost.Value, request.TraderId, output);

            if (output.Warnings?.Count > 0)
            {
                return output;
            }

            // Add repaired item to output object
            output.ProfileChanges[sessionID].Items.ChangedItems.Add(repairDetails.RepairedItem);

            // Add skill points for repairing weapons
            repairService.AddRepairSkillPoints(sessionID, repairDetails, pmcData);
        }

        return output;
    }

    /// <summary>
    ///     Handle Repair event
    ///     Repair with repair kit
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <param name="body">endpoint request data</param>
    /// <param name="pmcData">player profile</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse RepairWithKit(MongoId sessionId, RepairActionDataRequest body, PmcData pmcData)
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        // repair item
        var repairDetails = repairService.RepairItemByKit(sessionId, pmcData, body.RepairKitsInfo, body.Target.Value, output);

        repairService.AddBuffToItem(repairDetails, pmcData);

        // add repaired item to send to client
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(repairDetails.RepairedItem);

        // Add skill points for repairing items
        repairService.AddRepairSkillPoints(sessionId, repairDetails, pmcData);

        return output;
    }
}
