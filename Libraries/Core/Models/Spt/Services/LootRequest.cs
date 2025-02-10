using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Enums;

namespace Core.Models.Spt.Services;

public record LootRequest
{
    /// <summary>
    ///     Count of weapons to generate
    /// </summary>
    [JsonPropertyName("weaponPresetCount")]
    public MinMax<int>? WeaponPresetCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Count of armor to generate
    /// </summary>
    [JsonPropertyName("armorPresetCount")]
    public MinMax<int>? ArmorPresetCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Count of items to generate
    /// </summary>
    [JsonPropertyName("itemCount")]
    public MinMax<int>? ItemCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Count of sealed weapon crates to generate
    /// </summary>
    [JsonPropertyName("weaponCrateCount")]
    public MinMax<int>? WeaponCrateCount
    {
        get;
        set;
    }

    /// <summary>
    ///     Item tpl blacklist to exclude
    /// </summary>
    [JsonPropertyName("itemBlacklist")]
    public List<string>? ItemBlacklist
    {
        get;
        set;
    }

    /// <summary>
    ///     Item tpl whitelist to pick from
    /// </summary>
    [JsonPropertyName("itemTypeWhitelist")]
    public List<string>? ItemTypeWhitelist
    {
        get;
        set;
    }

    /// <summary>
    ///     key: item base type: value: max count
    /// </summary>
    [JsonPropertyName("itemLimits")]
    public Dictionary<string, int>? ItemLimits
    {
        get;
        set;
    }

    [JsonPropertyName("itemStackLimits")]
    public Dictionary<string, MinMax<int>>? ItemStackLimits
    {
        get;
        set;
    }

    /// <summary>
    ///     Allowed armor plate levels 2/3/4/5/6 for armor generated
    /// </summary>
    [JsonPropertyName("armorLevelWhitelist")]
    public List<int>? ArmorLevelWhitelist
    {
        get;
        set;
    }

    /// <summary>
    ///     Should boss items be included in allowed items
    /// </summary>
    [JsonPropertyName("allowBossItems")]
    public bool? AllowBossItems
    {
        get;
        set;
    }

    /// <summary>
    ///     Should item.json item reward blacklist be used
    /// </summary>
    [JsonPropertyName("useRewardItemBlacklist")]
    public bool? UseRewardItemBlacklist
    {
        get;
        set;
    }

    /// <summary>
    ///     Should forced loot be used instead of randomised loot
    /// </summary>
    [JsonPropertyName("useForcedLoot")]
    public bool? UseForcedLoot
    {
        get;
        set;
    }

    /// <summary>
    ///     Item tpls + count of items to force include
    /// </summary>
    [JsonPropertyName("forcedLoot")]
    public Dictionary<string, MinMax<int>>? ForcedLoot
    {
        get;
        set;
    }
}

public record AirdropLootRequest : LootRequest
{
    /// <summary>
    ///     Airdrop icon used by client to show crate type
    /// </summary>
    [JsonPropertyName("icon")]
    public AirdropTypeEnum? Icon
    {
        get;
        set;
    }
}
