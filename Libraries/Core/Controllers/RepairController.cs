using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Repair;
using Core.Routers;
using Core.Services;

namespace Core.Controllers;

[Injectable]
public class RepairController(
    EventOutputHolder _eventOutputHolder,
    RepairService _repairService
    )
{
    /// <summary>
    /// Handle TraderRepair event
    /// Repair with trader
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <param name="body">endpoint request data</param>
    /// <param name="pmcData">player profile</param>
    /// <returns>item event router action</returns>
    public ItemEventRouterResponse TraderRepair(
        string sessionID,
        TraderRepairActionDataRequest body,
        PmcData pmcData)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        // find the item to repair
        foreach (var repairItem in body.RepairItems) {
            var repairDetails = _repairService.RepairItemByTrader(sessionID, pmcData, repairItem, body.TId);

            _repairService.PayForRepair(
                sessionID,
                pmcData,
                repairItem.Id,
                repairDetails.RepairCost.Value,
                body.TId,
                output);

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
    /// Handle Repair event
    /// Repair with repair kit
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <param name="body">endpoint request data</param>
    /// <param name="pmcData">player profile</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse RepairWithKit(
        string sessionId,
        RepairActionDataRequest body,
        PmcData pmcData)
    {
        throw new NotImplementedException();
    }
}
