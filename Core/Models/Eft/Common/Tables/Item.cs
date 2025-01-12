using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class Item
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    [JsonPropertyName("slotId")]
    public string? SlotId { get; set; }

    [JsonPropertyName("location")]
    public object? Location { get; set; } // TODO: Can be IItemLocation or number
    
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class ItemLocation
{
    public float? X { get; set; }
    public float? Y { get; set; }
    public object? R { get; set; } // TODO: Can be string or number
    public bool? IsSearched { get; set; }

    /** SPT property? */
    public object? Rotation { get; set; } // TODO: Can be string or boolean
}

public class Upd
{
    public UpdBuff? Buff { get; set; }
    public int? OriginalStackObjectsCount { get; set; }
    public UpdTogglable? Togglable { get; set; }
    public UpdMap? Map { get; set; }
    public UpdTag? Tag { get; set; }

    /** SPT specific property, not made by BSG */
    [JsonPropertyName("sptPresetId")]
    public string? SptPresetId { get; set; }

    public UpdFaceShield? FaceShield { get; set; }
    public double? StackObjectsCount { get; set; }
    public bool? UnlimitedCount { get; set; }
    public UpdRepairable? Repairable { get; set; }
    public UpdRecodableComponent? RecodableComponent { get; set; }
    public UpdFireMode? FireMode { get; set; }
    public bool? SpawnedInSession { get; set; }
    public UpdLight? Light { get; set; }
    public UpdKey? Key { get; set; }
    public UpdResource? Resource { get; set; }
    public UpdSight? Sight { get; set; }
    public UpdMedKit? MedKit { get; set; }
    public UpdFoodDrink? FoodDrink { get; set; }
    public UpdDogtag? Dogtag { get; set; }
    public int? BuyRestrictionMax { get; set; }
    public int? BuyRestrictionCurrent { get; set; }
    public UpdFoldable? Foldable { get; set; }
    public UpdSideEffect? SideEffect { get; set; }
    public UpdRepairKit? RepairKit { get; set; }
    public UpdCultistAmulet? CultistAmulet { get; set; }
    public PinLockState? PinLockState { get; set; }
}

public enum PinLockState
{
    Free,
    Locked,
    Pinned
}

public class UpdBuff
{
    [JsonPropertyName("Rarity")]
    public string? Rarity { get; set; }

    [JsonPropertyName("BuffType")]
    public string? BuffType { get; set; }

    [JsonPropertyName("Value")]
    public int? Value { get; set; }

    [JsonPropertyName("ThresholdDurability")]
    public int? ThresholdDurability { get; set; }
}

public class UpdTogglable
{
    [JsonPropertyName("On")]
    public bool? On { get; set; }
}

public class UpdMap
{
    [JsonPropertyName("Markers")]
    public List<MapMarker>? Markers { get; set; }
}

public class MapMarker
{
    [JsonPropertyName("X")]
    public int? X { get; set; }

    [JsonPropertyName("Y")]
    public int? Y { get; set; }
}

public class UpdTag
{
    [JsonPropertyName("Color")]
    public int? Color { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}

public class UpdFaceShield
{
    [JsonPropertyName("Hits")]
    public int? Hits { get; set; }
}

public class UpdRepairable
{
    [JsonPropertyName("Durability")]
    public int? Durability { get; set; }

    [JsonPropertyName("MaxDurability")]
    public int? MaxDurability { get; set; }
}

public class UpdRecodableComponent
{
    [JsonPropertyName("IsEncoded")]
    public bool? IsEncoded { get; set; }
}

public class UpdMedKit
{
    [JsonPropertyName("HpResource")]
    public int? HpResource { get; set; }
}

public class UpdSight
{
    [JsonPropertyName("ScopesCurrentCalibPointIndexes")]
    public List<int>? ScopesCurrentCalibPointIndexes { get; set; }

    [JsonPropertyName("ScopesSelectedModes")]
    public List<int>? ScopesSelectedModes { get; set; }

    [JsonPropertyName("SelectedScope")]
    public int? SelectedScope { get; set; }
}

public class UpdFoldable
{
    [JsonPropertyName("Folded")]
    public bool? Folded { get; set; }
}

public class UpdFireMode
{
    [JsonPropertyName("FireMode")]
    public string? FireMode { get; set; }
}

public class UpdFoodDrink
{
    [JsonPropertyName("HpPercent")]
    public double? HpPercent { get; set; }
}

public class UpdKey
{
    [JsonPropertyName("NumberOfUsages")]
    public double? NumberOfUsages { get; set; }
}

public class UpdResource
{
    [JsonPropertyName("Value")]
    public double? Value { get; set; }

    [JsonPropertyName("UnitsConsumed")]
    public double? UnitsConsumed { get; set; }
}

public class UpdLight
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("SelectedMode")]
    public int? SelectedMode { get; set; }
}

public class UpdDogtag
{
    [JsonPropertyName("AccountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("ProfileId")]
    public string? ProfileId { get; set; }

    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public double? Level { get; set; }

    [JsonPropertyName("Time")]
    public string? Time { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("KillerAccountId")]
    public string? KillerAccountId { get; set; }

    [JsonPropertyName("KillerProfileId")]
    public string? KillerProfileId { get; set; }

    [JsonPropertyName("KillerName")]
    public string? KillerName { get; set; }

    [JsonPropertyName("WeaponName")]
    public string? WeaponName { get; set; }
}

public class UpdSideEffect
{
    [JsonPropertyName("Value")]
    public double? Value { get; set; }
}

public class UpdRepairKit
{
    [JsonPropertyName("Resource")]
    public double? Resource { get; set; }
}

public class UpdCultistAmulet
{
    [JsonPropertyName("NumberOfUsages")]
    public double? NumberOfUsages { get; set; }
}
