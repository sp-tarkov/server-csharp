using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Upd
{
    public UpdBuff? Buff
    {
        get;
        set;
    }

    public double? OriginalStackObjectsCount
    {
        get;
        set;
    }

    public UpdTogglable? Togglable
    {
        get;
        set;
    }

    public UpdMap? Map
    {
        get;
        set;
    }

    public UpdTag? Tag
    {
        get;
        set;
    }

    /// <summary>
    ///     SPT specific property, not made by BSG
    /// </summary>
    [JsonPropertyName("sptPresetId")]
    public string? SptPresetId
    {
        get;
        set;
    }

    public UpdFaceShield? FaceShield
    {
        get;
        set;
    }

    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? StackObjectsCount
    {
        get;
        set;
    } // TODO: LootDumpGen is outputting doubles, we can turn back to int once fixed

    public bool? UnlimitedCount
    {
        get;
        set;
    }

    public UpdRepairable? Repairable
    {
        get;
        set;
    }

    public UpdRecodableComponent? RecodableComponent
    {
        get;
        set;
    }

    public UpdFireMode? FireMode
    {
        get;
        set;
    }

    public bool? SpawnedInSession
    {
        get;
        set;
    }

    public UpdLight? Light
    {
        get;
        set;
    }

    public UpdKey? Key
    {
        get;
        set;
    }

    public UpdResource? Resource
    {
        get;
        set;
    }

    public UpdSight? Sight
    {
        get;
        set;
    }

    public UpdMedKit? MedKit
    {
        get;
        set;
    }

    public UpdFoodDrink? FoodDrink
    {
        get;
        set;
    }

    public UpdDogtag? Dogtag
    {
        get;
        set;
    }

    public int? BuyRestrictionMax
    {
        get;
        set;
    }

    public int? BuyRestrictionCurrent
    {
        get;
        set;
    }

    public UpdFoldable? Foldable
    {
        get;
        set;
    }

    public UpdSideEffect? SideEffect
    {
        get;
        set;
    }

    public UpdRepairKit? RepairKit
    {
        get;
        set;
    }

    public UpdCultistAmulet? CultistAmulet
    {
        get;
        set;
    }

    public PinLockState? PinLockState
    {
        get;
        set;
    }

    public Lockable? Lockable // LockableComponent in the client
    {
        get;
        set;
    }
}
