namespace Core.Models.Enums;

public record HideoutEventActions
{
    public const string HIDEOUT_UPGRADE = "HideoutUpgrade";  // worked
    public const string HIDEOUT_UPGRADE_COMPLETE = "HideoutUpgradeComplete";  // worked
    public const string HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS = "HideoutPutItemsInAreaSlots";  // worked
    public const string HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS = "HideoutTakeItemsFromAreaSlots";  // worked
    public const string HIDEOUT_TOGGLE_AREA = "HideoutToggleArea"; // worked
    public const string HIDEOUT_SINGLE_PRODUCTION_START = "HideoutSingleProductionStart"; // worked
    public const string HIDEOUT_SCAV_CASE_PRODUCTION_START = "HideoutScavCaseProductionStart"; // worked
    public const string HIDEOUT_CONTINUOUS_PRODUCTION_START = "HideoutContinuousProductionStart"; // worked
    public const string HIDEOUT_TAKE_PRODUCTION = "HideoutTakeProduction"; // worked
    public const string HIDEOUT_RECORD_SHOOTING_RANGE_POINTS = "RecordShootingRangePoints"; // worked
    public const string HIDEOUT_IMPROVE_AREA = "HideoutImproveArea";
    public const string HIDEOUT_CANCEL_PRODUCTION_COMMAND = "HideoutCancelProductionCommand";
    public const string HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START = "HideoutCircleOfCultistProductionStart"; // Borked
    public const string HIDEOUT_DELETE_PRODUCTION_COMMAND = "HideoutDeleteProductionCommand";
    public const string HIDEOUT_CUSTOMIZATION_APPLY_COMMAND = "HideoutCustomizationApply"; // worked
    public const string HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE = "HideoutCustomizationSetMannequinPose"; // worked
}
