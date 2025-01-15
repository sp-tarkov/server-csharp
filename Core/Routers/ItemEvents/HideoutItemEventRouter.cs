using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class HideoutItemEventRouter : ItemEventRouterDefinition
{
    private readonly HideoutCallbacks _hideoutCallbacks;

    public HideoutItemEventRouter
    (
        HideoutCallbacks hideoutCallbacks)
    {
        _hideoutCallbacks = hideoutCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new()
        {
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
            new HandledRoute(HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE, false)
        };
    }

    public override object HandleItemEvent(string url, PmcData pmcData, object body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url) {
            case HideoutEventActions.HIDEOUT_UPGRADE:
                return _hideoutCallbacks.Upgrade(pmcData, body as HideoutUpgradeRequestData, sessionID, output);
            case HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE:
                return _hideoutCallbacks.UpgradeComplete(pmcData, body as HideoutUpgradeCompleteRequestData, sessionID, output);
            case HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS:
                return _hideoutCallbacks.PutItemsInAreaSlots(pmcData, body as HideoutPutItemInRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS:
                return _hideoutCallbacks.TakeItemsFromAreaSlots(pmcData, body as HideoutTakeItemOutRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_TOGGLE_AREA:
                return _hideoutCallbacks.ToggleArea(pmcData, body as HideoutToggleAreaRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START:
                return _hideoutCallbacks.SingleProductionStart(pmcData, body as HideoutSingleProductionStartRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START:
                return _hideoutCallbacks.ScavCaseProductionStart(pmcData, body as HideoutScavCaseStartRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START:
                return _hideoutCallbacks.ContinuousProductionStart(pmcData, body as HideoutContinuousProductionStartRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_TAKE_PRODUCTION:
                return _hideoutCallbacks.TakeProduction(pmcData, body as HideoutTakeProductionRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS:
                return _hideoutCallbacks.RecordShootingRangePoints(pmcData, body as RecordShootingRangePoints, sessionID, output);
            case HideoutEventActions.HIDEOUT_IMPROVE_AREA:
                return _hideoutCallbacks.ImproveArea(pmcData, body as HideoutImproveAreaRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND:
                return _hideoutCallbacks.CancelProduction(pmcData, body as HideoutImproveAreaRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START:
                return _hideoutCallbacks.CicleOfCultistProductionStart(pmcData, body as HideoutCircleOfCultistProductionStartRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND:
                return _hideoutCallbacks.HideoutDeleteProductionCommand(pmcData, body as HideoutDeleteProductionRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND:
                return _hideoutCallbacks.HideoutCustomizationApplyCommand(pmcData, body as HideoutCustomizationApplyRequestData, sessionID);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE:
                return _hideoutCallbacks.HideoutCustomizationSetMannequinPose(pmcData, body as HideoutCustomizationSetMannequinPoseRequest, sessionID);
            default:
                throw new Exception($"HideoutItemEventRouter being used when it cant handle route {url}");
        }
    }
}
