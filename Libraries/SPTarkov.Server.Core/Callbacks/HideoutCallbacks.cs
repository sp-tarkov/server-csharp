using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.HideoutCallbacks)]
public class HideoutCallbacks(HideoutController hideoutController, ConfigServer configServer) : IOnUpdate
{
    private readonly HideoutConfig _hideoutConfig = configServer.GetConfig<HideoutConfig>();

    public Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun < _hideoutConfig.RunIntervalSeconds)
        {
            // Not enough time has passed since last run, exit early
            return Task.FromResult(false);
        }

        hideoutController.Update();

        return Task.FromResult(true);
    }

    /// <summary>
    ///     Handle HideoutUpgrade event
    /// </summary>
    public ItemEventRouterResponse Upgrade(
        PmcData pmcData,
        HideoutUpgradeRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.StartUpgrade(pmcData, request, sessionID, output);

        return output;
    }

    /// <summary>
    ///     Handle HideoutUpgradeComplete event
    /// </summary>
    public ItemEventRouterResponse UpgradeComplete(
        PmcData pmcData,
        HideoutUpgradeCompleteRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.UpgradeComplete(pmcData, request, sessionID, output);

        return output;
    }

    /// <summary>
    ///     Handle HideoutPutItemsInAreaSlots
    /// </summary>
    public ItemEventRouterResponse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData request, MongoId sessionID)
    {
        return hideoutController.PutItemsInAreaSlots(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutTakeItemsFromAreaSlots event
    /// </summary>
    public ItemEventRouterResponse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData request, MongoId sessionID)
    {
        return hideoutController.TakeItemsFromAreaSlots(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutToggleArea event
    /// </summary>
    public ItemEventRouterResponse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData request, MongoId sessionID)
    {
        return hideoutController.ToggleArea(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutSingleProductionStart event
    /// </summary>
    public ItemEventRouterResponse SingleProductionStart(
        PmcData pmcData,
        HideoutSingleProductionStartRequestData request,
        MongoId sessionID
    )
    {
        return hideoutController.SingleProductionStart(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutScavCaseProductionStart event
    /// </summary>
    public ItemEventRouterResponse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData request, MongoId sessionID)
    {
        return hideoutController.ScavCaseProductionStart(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutContinuousProductionStart
    /// </summary>
    public ItemEventRouterResponse ContinuousProductionStart(
        PmcData pmcData,
        HideoutContinuousProductionStartRequestData request,
        MongoId sessionID
    )
    {
        return hideoutController.ContinuousProductionStart(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutTakeProduction event
    /// </summary>
    public ItemEventRouterResponse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData request, MongoId sessionID)
    {
        return hideoutController.TakeProduction(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle HideoutQuickTimeEvent
    /// </summary>
    public ItemEventRouterResponse HandleQTEEvent(
        PmcData pmcData,
        HandleQTEEventRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.HandleQTEEventOutcome(sessionID, pmcData, request, output);

        return output;
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    public ItemEventRouterResponse RecordShootingRangePoints(
        PmcData pmcData,
        RecordShootingRangePoints request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.RecordShootingRangePoints(sessionID, pmcData, request);

        return output;
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    public ItemEventRouterResponse ImproveArea(PmcData pmcData, HideoutImproveAreaRequestData request, MongoId sessionID)
    {
        return hideoutController.ImproveArea(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCancelProductionCommand
    /// </summary>
    public ItemEventRouterResponse CancelProduction(PmcData pmcData, HideoutCancelProductionRequestData request, MongoId sessionID)
    {
        return hideoutController.CancelProduction(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCircleOfCultistProductionStart
    /// </summary>
    public ItemEventRouterResponse CicleOfCultistProductionStart(
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request,
        MongoId sessionID
    )
    {
        return hideoutController.CircleOfCultistProductionStart(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutDeleteProductionCommand
    /// </summary>
    public ItemEventRouterResponse HideoutDeleteProductionCommand(
        PmcData pmcData,
        HideoutDeleteProductionRequestData request,
        MongoId sessionID
    )
    {
        return hideoutController.HideoutDeleteProductionCommand(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCustomizationApply
    /// </summary>
    public ItemEventRouterResponse HideoutCustomizationApplyCommand(
        PmcData pmcData,
        HideoutCustomizationApplyRequestData request,
        MongoId sessionID
    )
    {
        return hideoutController.HideoutCustomizationApply(sessionID, pmcData, request);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - hideoutCustomizationSetMannequinPose
    /// </summary>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(
        PmcData pmcData,
        HideoutCustomizationSetMannequinPoseRequest request,
        MongoId sessionId
    )
    {
        return hideoutController.HideoutCustomizationSetMannequinPose(sessionId, pmcData, request);
    }
}
