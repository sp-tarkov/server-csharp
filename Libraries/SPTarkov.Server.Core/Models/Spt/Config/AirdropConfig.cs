using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record AirdropConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-airdrop";

    [JsonPropertyName("airdropTypeWeightings")]
    public required Dictionary<SptAirdropTypeEnum, double> AirdropTypeWeightings
    {
        get;
        set;
    }

    /// <summary>
    ///     What rewards will the loot crate contain, keyed by drop type e.g. mixed/weaponArmor/foodMedical/barter
    /// </summary>
    [JsonPropertyName("loot")]
    public required Dictionary<string, AirdropLoot> Loot
    {
        get;
        set;
    }

    [JsonPropertyName("customAirdropMapping")]
    public required Dictionary<string, SptAirdropTypeEnum> CustomAirdropMapping
    {
        get;
        set;
    }
}

/// <summary>
///     Loot inside crate
/// </summary>
public record AirdropLoot
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("icon")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AirdropTypeEnum Icon
    {
        get;
        set;
    }

    /// <summary>
    ///     Min/max of weapons inside crate
    /// </summary>
    [JsonPropertyName("weaponPresetCount")]
    public required MinMax<int> WeaponPresetCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Min/max of armors (head/chest/rig) inside crate
    /// </summary>
    [JsonPropertyName("armorPresetCount")]
    public required MinMax<int> ArmorPresetCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Min/max of items inside crate
    /// </summary>
    [JsonPropertyName("itemCount")]
    public required MinMax<int> ItemCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Min/max of sealed weapon boxes inside crate
    /// </summary>
    [JsonPropertyName("weaponCrateCount")]
    public required MinMax<int> WeaponCrateCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Items to never allow - tpls
    /// </summary>
    [JsonPropertyName("itemBlacklist")]
    public required List<string> ItemBlacklist
    {
        get;
        set;
    }

    /// <summary>
    ///     Item type (parentId) to allow inside crate
    /// </summary>
    [JsonPropertyName("itemTypeWhitelist")]
    public required List<string> ItemTypeWhitelist
    {
        get;
        set;
    }

    /// <summary>
    ///     Item type/ item tpls to limit count of inside crate - key: item base type: value: max count
    /// </summary>
    [JsonPropertyName("itemLimits")]
    public required Dictionary<string, int> ItemLimits
    {
        get;
        set;
    }

    /// <summary>
    ///     Items to limit stack size of key: item tpl value: min/max stack size
    /// </summary>
    [JsonPropertyName("itemStackLimits")]
    public required Dictionary<string, MinMax<int>> ItemStackLimits
    {
        get;
        set;
    }

    /// <summary>
    ///     Armor levels to allow inside crate e.g. [4,5,6]
    /// </summary>
    [JsonPropertyName("armorLevelWhitelist")]
    public List<int>? ArmorLevelWhitelist
    {
        get;
        set;
    }

    /// <summary>
    ///     Should boss items be added to airdrop crate
    /// </summary>
    [JsonPropertyName("allowBossItems")]
    public bool AllowBossItems
    {
        get;
        set;
    }

    [JsonPropertyName("useForcedLoot")]
    public bool UseForcedLoot
    {
        get;
        set;
    }

    [JsonPropertyName("forcedLoot")]
    public Dictionary<string, MinMax<int>>? ForcedLoot
    {
        get;
        set;
    }

    [JsonPropertyName("useRewardItemBlacklist")]
    public bool UseRewardItemBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("blockSeasonalItemsOutOfSeason")]
    public bool BlockSeasonalItemsOutOfSeason
    {
        get;
        set;
    }
}
