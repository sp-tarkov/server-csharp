namespace Core.Models.Enums;

public record ItemEventActions
{
    public const string MOVE = "Move";
    public const string REMOVE = "Remove";
    public const string SPLIT = "Split";
    public const string MERGE = "Merge";
    public const string TRANSFER = "Transfer";
    public const string SWAP = "Swap";
    public const string FOLD = "Fold";
    public const string TOGGLE = "Toggle";
    public const string TAG = "Tag";
    public const string BIND = "Bind";
    public const string UNBIND = "Unbind";
    public const string EXAMINE = "Examine";
    public const string READ_ENCYCLOPEDIA = "ReadEncyclopedia";
    public const string APPLY_INVENTORY_CHANGES = "ApplyInventoryChanges";
    public const string CREATE_MAP_MARKER = "CreateMapMarker";
    public const string DELETE_MAP_MARKER = "DeleteMapMarker";
    public const string EDIT_MAP_MARKER = "EditMapMarker";
    public const string OPEN_RANDOM_LOOT_CONTAINER = "OpenRandomLootContainer";
    public const string HIDEOUT_QTE_EVENT = "HideoutQuickTimeEvent";
    public const string SAVE_WEAPON_BUILD = "SaveWeaponBuild";
    public const string REMOVE_WEAPON_BUILD = "RemoveWeaponBuild";
    public const string REMOVE_BUILD = "RemoveBuild";
    public const string SAVE_EQUIPMENT_BUILD = "SaveEquipmentBuild";
    public const string REMOVE_EQUIPMENT_BUILD = "RemoveEquipmentBuild";
    public const string REDEEM_PROFILE_REWARD = "RedeemProfileReward";
    public const string SET_FAVORITE_ITEMS = "SetFavoriteItems";
    public const string QUEST_FAIL = "QuestFail";
    public const string PIN_LOCK = "PinLock";
}
