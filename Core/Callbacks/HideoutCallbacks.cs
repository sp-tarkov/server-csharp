using Core.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnUpdate), TypePriority = OnUpdateOrder.HideoutCallbacks)]
public class HideoutCallbacks : OnUpdate
{
    protected HideoutController _hideoutController;
    protected ConfigServer _configServer;
    protected HideoutConfig _hideoutConfig;

    public HideoutCallbacks
    (
        HideoutController hideoutController,
        ConfigServer configServer
    )
    {
        _hideoutController = hideoutController;
        _configServer = configServer;
        _hideoutConfig = configServer.GetConfig<HideoutConfig>(ConfigTypes.HIDEOUT);
    }

    /// <summary>
    /// Handle HideoutUpgrade event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse Upgrade(PmcData pmcData, HideoutUpgradeRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _hideoutController.StartUpgrade(pmcData, info, sessionID, output);
        // TODO: HideoutController is not implemented rn
        return output;
    }

    /// <summary>
    /// Handle HideoutUpgradeComplete event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse UpgradeComplete(PmcData pmcData, HideoutUpgradeCompleteRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        // _hideoutController.UpgradeComplete(pmcData, info, sessionID, output);
        // TODO: HideoutController is not implemented rn
        return output;
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
        // return _hideoutController.PutItemsInAreaSlots(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.TakeItemsFromAreaSlots(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.ToggleArea(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.SingleProductionStart(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.ScavCaseProductionStart(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.ContinuousProductionStart(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.TakeProduction(pmcData, info, sessionID);
        // TODO: HideoutController is not implemented rn
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
        // _hideoutController.HandleQTEEventOutcome(sessionID, pmcData, info, output);
        // TODO: HideoutController is not implemented rn
        return output;
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
        // _hideoutController.RecordShootingRangePoints(sessionID, pmcData, info);
        // TODO: HideoutController is not implemented rn
        return output;
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
        // return _hideoutController.ImproveArea(sessionID, pmcData, info);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.CancelProduction(sessionID, pmcData, info);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.CicleOfCultistProductionStart(sessionID, pmcData, info);
        // TODO: HideoutController is not implemented rn
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - HideoutDeleteProductionCommand
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutDeleteProductionCommand(PmcData pmcData, HideoutDeleteProductionRequestData info, string sessionID)
    {
        // return _hideoutController.HideoutDeleteProductionCommand(sessionID, pmcData, info);
        // TODO: HideoutController is not implemented rn
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
        // return _hideoutController.HideoutCustomizationApply(sessionID, pmcData, info);
        // TODO: HideoutController is not implemented rn
        throw new NotImplementedException();
    }

    public async Task<bool> OnUpdate(long timeSinceLastRun)
    {
        if (timeSinceLastRun > _hideoutConfig.RunIntervalSeconds)
        {
            // TODO
            // _hideoutController.Update();
            return true;
        }

        return false;
    }

    public string GetRoute()
    {
        return "spt-hideout";
    }
}
