using System.Collections.Generic;
using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Enums;

namespace Core.Models.Spt.Config;

public record AirdropConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-airdrop";

    [JsonPropertyName("airdropTypeWeightings")]
    public Dictionary<SptAirdropTypeEnum, double> AirdropTypeWeightings { get; set; }

    /// <summary>
    /// What rewards will the loot crate contain, keyed by drop type e.g. mixed/weaponArmor/foodMedical/barter
    /// </summary>
    [JsonPropertyName("loot")]
    public Dictionary<string, AirdropLoot> Loot { get; set; }

    [JsonPropertyName("customAirdropMapping")]
    public Dictionary<string, SptAirdropTypeEnum> CustomAirdropMapping { get; set; }
}

/// <summary>
/// Chance map will have an airdrop occur out of 100 - locations not included count as 0%
/// </summary>
public record AirdropChancePercent
{
    [JsonPropertyName("bigmap")]
    public double Bigmap { get; set; }

    [JsonPropertyName("woods")]
    public double Woods { get; set; }

    [JsonPropertyName("lighthouse")]
    public double Lighthouse { get; set; }

    [JsonPropertyName("shoreline")]
    public double Shoreline { get; set; }

    [JsonPropertyName("interchange")]
    public double Interchange { get; set; }

    [JsonPropertyName("reserve")]
    public double Reserve { get; set; }

    [JsonPropertyName("tarkovStreets")]
    public double TarkovStreets { get; set; }

    [JsonPropertyName("sandbox")]
    public double Sandbox { get; set; }
}

/// <summary>
/// Loot inside crate
/// </summary>
public record AirdropLoot
{
    [JsonPropertyName("icon")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AirdropTypeEnum Icon { get; set; }

    /// <summary>
    /// Min/max of weapons inside crate
    /// </summary>
    [JsonPropertyName("weaponPresetCount")]
    public MinMax? WeaponPresetCount { get; set; }

    /// <summary>
    /// Min/max of armors (head/chest/rig) inside crate
    /// </summary>
    [JsonPropertyName("armorPresetCount")]
    public MinMax? ArmorPresetCount { get; set; }

    /// <summary>
    /// Min/max of items inside crate
    /// </summary>
    [JsonPropertyName("itemCount")]
    public MinMax ItemCount { get; set; }

    /// <summary>
    /// Min/max of sealed weapon boxes inside crate
    /// </summary>
    [JsonPropertyName("weaponCrateCount")]
    public MinMax WeaponCrateCount { get; set; }

    /// <summary>
    /// Items to never allow - tpls
    /// </summary>
    [JsonPropertyName("itemBlacklist")]
    public List<string> ItemBlacklist { get; set; }

    /// <summary>
    /// Item type (parentId) to allow inside crate
    /// </summary>
    [JsonPropertyName("itemTypeWhitelist")]
    public List<string> ItemTypeWhitelist { get; set; }

    /// <summary>
    /// Item type/ item tpls to limit count of inside crate - key: item base type: value: max count
    /// </summary>
    [JsonPropertyName("itemLimits")]
    public Dictionary<string, double> ItemLimits { get; set; }

    /// <summary>
    /// Items to limit stack size of key: item tpl value: min/max stack size
    /// </summary>
    [JsonPropertyName("itemStackLimits")]
    public Dictionary<string, MinMax> ItemStackLimits { get; set; }

    /// <summary>
    /// Armor levels to allow inside crate e.g. [4,5,6]
    /// </summary>
    [JsonPropertyName("armorLevelWhitelist")]
    public List<int>? ArmorLevelWhitelist { get; set; }

    /// <summary>
    /// Should boss items be added to airdrop crate
    /// </summary>
    [JsonPropertyName("allowBossItems")]
    public bool AllowBossItems { get; set; }

    [JsonPropertyName("useForcedLoot")]
    public bool? UseForcedLoot { get; set; }

    [JsonPropertyName("forcedLoot")]
    public Dictionary<string, MinMax>? ForcedLoot { get; set; }
}
