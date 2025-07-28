using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class HideoutItemEventRouter(HideoutCallbacks hideoutCallbacks) : ItemEventRouterDefinition
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return
        [
            new HandledRoute(HideoutEventActions.HIDEOUT_UPGRADE, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_TOGGLE_AREA, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_TAKE_PRODUCTION, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_IMPROVE_AREA, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND, false),
            new HandledRoute(HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE, false),
        ];
    }

    public override ValueTask<ItemEventRouterResponse> HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        switch (url)
        {
            case HideoutEventActions.HIDEOUT_UPGRADE:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.Upgrade(pmcData, body as HideoutUpgradeRequestData, sessionID, output)
                );
            case HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.UpgradeComplete(pmcData, body as HideoutUpgradeCompleteRequestData, sessionID, output)
                );
            case HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.PutItemsInAreaSlots(pmcData, body as HideoutPutItemInRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.TakeItemsFromAreaSlots(pmcData, body as HideoutTakeItemOutRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_TOGGLE_AREA:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.ToggleArea(pmcData, body as HideoutToggleAreaRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.SingleProductionStart(pmcData, body as HideoutSingleProductionStartRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.ScavCaseProductionStart(pmcData, body as HideoutScavCaseStartRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.ContinuousProductionStart(pmcData, body as HideoutContinuousProductionStartRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_TAKE_PRODUCTION:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.TakeProduction(pmcData, body as HideoutTakeProductionRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.RecordShootingRangePoints(pmcData, body as RecordShootingRangePoints, sessionID, output)
                );
            case HideoutEventActions.HIDEOUT_IMPROVE_AREA:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.ImproveArea(pmcData, body as HideoutImproveAreaRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.CancelProduction(pmcData, body as HideoutCancelProductionRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.CicleOfCultistProductionStart(
                        pmcData,
                        body as HideoutCircleOfCultistProductionStartRequestData,
                        sessionID
                    )
                );
            case HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.HideoutDeleteProductionCommand(pmcData, body as HideoutDeleteProductionRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.HideoutCustomizationApplyCommand(pmcData, body as HideoutCustomizationApplyRequestData, sessionID)
                );
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE:
                return new ValueTask<ItemEventRouterResponse>(
                    hideoutCallbacks.HideoutCustomizationSetMannequinPose(
                        pmcData,
                        body as HideoutCustomizationSetMannequinPoseRequest,
                        sessionID
                    )
                );
            default:
                throw new Exception($"HideoutItemEventRouter being used when it cant handle route {url}");
        }
    }
}
