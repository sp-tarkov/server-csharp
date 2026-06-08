using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.HideoutCallbacks)]
public class HideoutCallbacks(HideoutController hideoutController, HideoutConfig hideoutConfig) : IOnUpdate
{
    public Task<bool> OnUpdateAsync(long secondsSinceLastRun, CancellationToken cancellationToken)
    {
        if (secondsSinceLastRun < hideoutConfig.RunIntervalSeconds)
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
    public ValueTask<ItemEventRouterResponse> Upgrade(
        PmcData pmcData,
        HideoutUpgradeRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.StartUpgrade(pmcData, request, sessionID, output);

        return new ValueTask<ItemEventRouterResponse>(output);
    }

    /// <summary>
    ///     Handle HideoutUpgradeComplete event
    /// </summary>
    public ValueTask<ItemEventRouterResponse> UpgradeComplete(
        PmcData pmcData,
        HideoutUpgradeCompleteRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.UpgradeComplete(pmcData, request, sessionID, output);

        return new ValueTask<ItemEventRouterResponse>(output);
    }

    /// <summary>
    ///     Handle HideoutPutItemsInAreaSlots
    /// </summary>
    public ValueTask<ItemEventRouterResponse> PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData request, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.PutItemsInAreaSlots(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutTakeItemsFromAreaSlots event
    /// </summary>
    public ValueTask<ItemEventRouterResponse> TakeItemsFromAreaSlots(
        PmcData pmcData,
        HideoutTakeItemOutRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.TakeItemsFromAreaSlots(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutToggleArea event
    /// </summary>
    public ValueTask<ItemEventRouterResponse> ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData request, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.ToggleArea(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutSingleProductionStart event
    /// </summary>
    public ValueTask<ItemEventRouterResponse> SingleProductionStart(
        PmcData pmcData,
        HideoutSingleProductionStartRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.SingleProductionStart(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutScavCaseProductionStart event
    /// </summary>
    public ValueTask<ItemEventRouterResponse> ScavCaseProductionStart(
        PmcData pmcData,
        HideoutScavCaseStartRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.ScavCaseProductionStart(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutContinuousProductionStart
    /// </summary>
    public ValueTask<ItemEventRouterResponse> ContinuousProductionStart(
        PmcData pmcData,
        HideoutContinuousProductionStartRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.ContinuousProductionStart(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutTakeProduction event
    /// </summary>
    public ValueTask<ItemEventRouterResponse> TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData request, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.TakeProduction(pmcData, request, sessionID));
    }

    /// <summary>
    ///     Handle HideoutQuickTimeEvent
    /// </summary>
    public ValueTask<ItemEventRouterResponse> HandleQTEEvent(
        PmcData pmcData,
        HandleQTEEventRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.HandleQTEEventOutcome(sessionID, pmcData, request, output);

        return new ValueTask<ItemEventRouterResponse>(output);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    public ValueTask<ItemEventRouterResponse> RecordShootingRangePoints(
        PmcData pmcData,
        RecordShootingRangePoints request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        hideoutController.RecordShootingRangePoints(sessionID, pmcData, request);

        return new ValueTask<ItemEventRouterResponse>(output);
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - RecordShootingRangePoints
    /// </summary>
    public ValueTask<ItemEventRouterResponse> ImproveArea(PmcData pmcData, HideoutImproveAreaRequestData request, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.ImproveArea(sessionID, pmcData, request));
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCancelProductionCommand
    /// </summary>
    public ValueTask<ItemEventRouterResponse> CancelProduction(
        PmcData pmcData,
        HideoutCancelProductionRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.CancelProduction(sessionID, pmcData, request));
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCircleOfCultistProductionStart
    /// </summary>
    public ValueTask<ItemEventRouterResponse> CicleOfCultistProductionStart(
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.CircleOfCultistProductionStart(sessionID, pmcData, request));
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutDeleteProductionCommand
    /// </summary>
    public ValueTask<ItemEventRouterResponse> HideoutDeleteProductionCommand(
        PmcData pmcData,
        HideoutDeleteProductionRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.HideoutDeleteProductionCommand(sessionID, pmcData, request));
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutCustomizationApply
    /// </summary>
    public ValueTask<ItemEventRouterResponse> HideoutCustomizationApplyCommand(
        PmcData pmcData,
        HideoutCustomizationApplyRequestData request,
        MongoId sessionID
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.HideoutCustomizationApply(sessionID, pmcData, request));
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - hideoutCustomizationSetMannequinPose
    /// </summary>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> HideoutCustomizationSetMannequinPose(
        PmcData pmcData,
        HideoutCustomizationSetMannequinPoseRequest request,
        MongoId sessionId
    )
    {
        return new ValueTask<ItemEventRouterResponse>(hideoutController.HideoutCustomizationSetMannequinPose(sessionId, pmcData, request));
    }
}
