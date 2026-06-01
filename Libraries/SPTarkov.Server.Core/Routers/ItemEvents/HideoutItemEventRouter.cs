using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class HideoutItemEventRouter(HideoutCallbacks hideoutCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<HideoutUpgradeRequestData>(
            HideoutEventActions.HIDEOUT_UPGRADE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.Upgrade(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<HideoutUpgradeCompleteRequestData>(
            HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.UpgradeComplete(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<HideoutPutItemInRequestData>(
            HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.PutItemsInAreaSlots(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutTakeItemOutRequestData>(
            HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.TakeItemsFromAreaSlots(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutToggleAreaRequestData>(
            HideoutEventActions.HIDEOUT_TOGGLE_AREA,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.ToggleArea(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutSingleProductionStartRequestData>(
            HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.SingleProductionStart(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutScavCaseStartRequestData>(
            HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.ScavCaseProductionStart(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutContinuousProductionStartRequestData>(
            HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.ContinuousProductionStart(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutTakeProductionRequestData>(
            HideoutEventActions.HIDEOUT_TAKE_PRODUCTION,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.TakeProduction(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<RecordShootingRangePoints>(
            HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.RecordShootingRangePoints(pmcData, body, sessionID, output));
            }
        ),
        new ItemRouteAction<HideoutImproveAreaRequestData>(
            HideoutEventActions.HIDEOUT_IMPROVE_AREA,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.ImproveArea(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutCancelProductionRequestData>(
            HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.CancelProduction(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutCircleOfCultistProductionStartRequestData>(
            HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.CicleOfCultistProductionStart(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutDeleteProductionRequestData>(
            HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.HideoutDeleteProductionCommand(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutCustomizationApplyRequestData>(
            HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(hideoutCallbacks.HideoutCustomizationApplyCommand(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<HideoutCustomizationSetMannequinPoseRequest>(
            HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.HideoutCustomizationSetMannequinPose(pmcData, body, sessionID)
                );
            }
        ),
    ]) { }
