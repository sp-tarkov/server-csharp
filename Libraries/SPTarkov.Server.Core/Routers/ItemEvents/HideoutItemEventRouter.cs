using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class HideoutItemEventRouter(HideoutCallbacks hideoutCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<HideoutUpgradeRequestData>(
            HideoutEventActions.HIDEOUT_UPGRADE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.Upgrade(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<HideoutUpgradeCompleteRequestData>(
            HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.UpgradeComplete(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<HideoutPutItemInRequestData>(
            HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.PutItemsInAreaSlots(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutTakeItemOutRequestData>(
            HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.TakeItemsFromAreaSlots(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutToggleAreaRequestData>(
            HideoutEventActions.HIDEOUT_TOGGLE_AREA,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await hideoutCallbacks.ToggleArea(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutSingleProductionStartRequestData>(
            HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.SingleProductionStart(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutScavCaseStartRequestData>(
            HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.ScavCaseProductionStart(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutContinuousProductionStartRequestData>(
            HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.ContinuousProductionStart(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutTakeProductionRequestData>(
            HideoutEventActions.HIDEOUT_TAKE_PRODUCTION,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.TakeProduction(pmcData, body, sessionID)
        ),
        new ItemRouteAction<RecordShootingRangePoints>(
            HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.RecordShootingRangePoints(pmcData, body, sessionID, output)
        ),
        new ItemRouteAction<HideoutImproveAreaRequestData>(
            HideoutEventActions.HIDEOUT_IMPROVE_AREA,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await hideoutCallbacks.ImproveArea(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutCancelProductionRequestData>(
            HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.CancelProduction(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutCircleOfCultistProductionStartRequestData>(
            HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.CicleOfCultistProductionStart(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutDeleteProductionRequestData>(
            HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.HideoutDeleteProductionCommand(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutCustomizationApplyRequestData>(
            HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.HideoutCustomizationApplyCommand(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HideoutCustomizationSetMannequinPoseRequest>(
            HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await hideoutCallbacks.HideoutCustomizationSetMannequinPose(pmcData, body, sessionID)
        ),
    ]) { }
