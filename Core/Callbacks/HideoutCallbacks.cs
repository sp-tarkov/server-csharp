using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;

namespace Core.Callbacks;

public class HideoutCallbacks : OnUpdate
{
    private HideoutConfig _hideoutConfig;

    public HideoutCallbacks()
    {
    }

    /// <summary>
    /// Handle HideoutUpgrade event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse Upgrade(PmcData pmcData, HideoutUpgradeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutUpgradeComplete event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse UpgradeComplete(PmcData pmcData, HideoutUpgradeCompleteRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutPutItemsInAreaSlots
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutTakeItemsFromAreaSlots event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutToggleArea event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutSingleProductionStart event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SingleProductionStart(PmcData pmcData, HideoutSingleProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutScavCaseProductionStart event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutContinuousProductionStart
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ContinuousProductionStart(PmcData pmcData, HideoutContinuousProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutTakeProduction event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle HideoutQuickTimeEvent
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse HandleQTEEvent(PmcData pmcData, HandleQTEEventRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RecordShootingRangePoints(PmcData pmcData, RecordShootingRangePoints info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ImproveArea(PmcData pmcData, HideoutImproveAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - HideoutCancelProductionCommand
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse CancelProduction(PmcData pmcData, HideoutImproveAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - HideoutCircleOfCultistProductionStart
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse CicleOfCultistProductionStart(PmcData pmcData, HideoutCircleOfCultistProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - HideoutDeleteProductionCommand
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutDeleteProductionRequestData(PmcData pmcData, HideoutDeleteProductionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - HideoutCustomizationApply
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutCustomizationApplyCommand(PmcData pmcData, HideoutCustomizationApplyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> OnUpdate(long timeSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}