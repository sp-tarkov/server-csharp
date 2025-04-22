using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class RepairController(
    EventOutputHolder _eventOutputHolder,
    RepairService _repairService
)
{
    /// <summary>
    ///     Handle TraderRepair event
    ///     Repair with trader
    /// </summary>
    /// <param name="sessionID">session id</param>
    /// <param name="body">endpoint request data</param>
    /// <param name="pmcData">player profile</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse TraderRepair(
        string sessionID,
        TraderRepairActionDataRequest body,
        PmcData pmcData)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        // find the item to repair
        foreach (var repairItem in body.RepairItems)
        {
            var repairDetails = _repairService.RepairItemByTrader(sessionID, pmcData, repairItem, body.TId);

            _repairService.PayForRepair(
                sessionID,
                pmcData,
                repairItem.Id,
                repairDetails.RepairCost.Value,
                body.TId,
                output
            );

            if (output.Warnings?.Count > 0)
            {
                return output;
            }

            // Add repaired item to output object
            output.ProfileChanges[sessionID].Items.ChangedItems.Add(repairDetails.RepairedItem);

            // Add skill points for repairing weapons
            _repairService.AddRepairSkillPoints(sessionID, repairDetails, pmcData);
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
    public ItemEventRouterResponse RepairWithKit(
        string sessionId,
        RepairActionDataRequest body,
        PmcData pmcData)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        // repair item
        var repairDetails = _repairService.RepairItemByKit(
            sessionId,
            pmcData,
            body.RepairKitsInfo,
            body.Target,
            output
        );

        _repairService.AddBuffToItem(repairDetails, pmcData);

        // add repaired item to send to client
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(repairDetails.RepairedItem);

        // Add skill points for repairing items
        _repairService.AddRepairSkillPoints(sessionId, repairDetails, pmcData);

        return output;
    }
}
