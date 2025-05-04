using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(
    InjectableTypeOverride = typeof(IOnUpdate),
    TypePriority = OnUpdateOrder.HideoutCallbacks
)]
[Injectable(InjectableTypeOverride = typeof(HideoutCallbacks))]
public class HideoutCallbacks(HideoutController _hideoutController, ConfigServer _configServer)
    : IOnUpdate
{
    private readonly HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();

    public bool OnUpdate(long timeSinceLastRun)
    {
        if (timeSinceLastRun > _hideoutConfig.RunIntervalSeconds)
        {
            _hideoutController.Update();
            return true;
        }

        return false;
    }

    public string GetRoute()
    {
        return "spt-hideout";
    }

    /// <summary>
    ///     Handle HideoutUpgrade event
    /// </summary>
    public ItemEventRouterResponse Upgrade(
        PmcData pmcData,
        HideoutUpgradeRequestData request,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        _hideoutController.StartUpgrade(pmcData, request, sessionID, output);

        return output;
    }

    /// <summary>
    ///     Handle HideoutUpgradeComplete event
    /// </summary>
    public ItemEventRouterResponse UpgradeComplete(
        PmcData pmcData,
        HideoutUpgradeCompleteRequestData request,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        _hideoutController.UpgradeComplete(pmcData, request, sessionID, output);

        return output;
    }

    /// <summary>
    ///     Handle HideoutPutItemsInAreaSlots
    /// </summary>
    public ItemEventRouterResponse PutItemsInAreaSlots(
        PmcData pmcData,
        HideoutPutItemInRequestData request,
        string sessionID
    )
    {
        return _hideoutController.PutItemsInAreaSlots(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutTakeItemsFromAreaSlots event
    /// </summary>
    public ItemEventRouterResponse TakeItemsFromAreaSlots(
        PmcData pmcData,
        HideoutTakeItemOutRequestData request,
        string sessionID
    )
    {
        return _hideoutController.TakeItemsFromAreaSlots(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutToggleArea event
    /// </summary>
    public ItemEventRouterResponse ToggleArea(
        PmcData pmcData,
        HideoutToggleAreaRequestData request,
        string sessionID
    )
    {
        return _hideoutController.ToggleArea(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutSingleProductionStart event
    /// </summary>
    public ItemEventRouterResponse SingleProductionStart(
        PmcData pmcData,
        HideoutSingleProductionStartRequestData request,
        string sessionID
    )
    {
        return _hideoutController.SingleProductionStart(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutScavCaseProductionStart event
    /// </summary>
    public ItemEventRouterResponse ScavCaseProductionStart(
        PmcData pmcData,
        HideoutScavCaseStartRequestData request,
        string sessionID
    )
    {
        return _hideoutController.ScavCaseProductionStart(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutContinuousProductionStart
    /// </summary>
    public ItemEventRouterResponse ContinuousProductionStart(
        PmcData pmcData,
        HideoutContinuousProductionStartRequestData request,
        string sessionID
    )
    {
        return _hideoutController.ContinuousProductionStart(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutTakeProduction event
    /// </summary>
    public ItemEventRouterResponse TakeProduction(
        PmcData pmcData,
        HideoutTakeProductionRequestData request,
        string sessionID
    )
    {
        return _hideoutController.TakeProduction(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutQuickTimeEvent
    /// </summary>
    public ItemEventRouterResponse HandleQTEEvent(
        PmcData pmcData,
        HandleQTEEventRequestData request,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        _hideoutController.HandleQTEEventOutcome(sessionID, pmcData, request, output);

        return output;
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    public ItemEventRouterResponse RecordShootingRangePoints(
        PmcData pmcData,
        RecordShootingRangePoints request,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        _hideoutController.RecordShootingRangePoints(sessionID, pmcData, request);

        return output;
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    public ItemEventRouterResponse ImproveArea(
        PmcData pmcData,
        HideoutImproveAreaRequestData request,
        string sessionID
    )
    {
        return _hideoutController.ImproveArea(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCancelProductionCommand
    /// </summary>
    public ItemEventRouterResponse CancelProduction(
        PmcData pmcData,
        HideoutCancelProductionRequestData request,
        string sessionID
    )
    {
        return _hideoutController.CancelProduction(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCircleOfCultistProductionStart
    /// </summary>
    public ItemEventRouterResponse CicleOfCultistProductionStart(
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request,
        string sessionID
    )
    {
        return _hideoutController.CicleOfCultistProductionStart(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutDeleteProductionCommand
    /// </summary>
    public ItemEventRouterResponse HideoutDeleteProductionCommand(
        PmcData pmcData,
        HideoutDeleteProductionRequestData request,
        string sessionID
    )
    {
        return _hideoutController.HideoutDeleteProductionCommand(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCustomizationApply
    /// </summary>
    public ItemEventRouterResponse HideoutCustomizationApplyCommand(
        PmcData pmcData,
        HideoutCustomizationApplyRequestData request,
        string sessionID
    )
    {
        return _hideoutController.HideoutCustomizationApply(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - hideoutCustomizationSetMannequinPose
    /// </summary>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(
        PmcData pmcData,
        HideoutCustomizationSetMannequinPoseRequest request,
        string sessionId
    )
    {
        return _hideoutController.HideoutCustomizationSetMannequinPose(sessionId, pmcData, request);
    }
}
