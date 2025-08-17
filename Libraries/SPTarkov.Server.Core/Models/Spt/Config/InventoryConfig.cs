using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record InventoryConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind { get; set; } = "spt-inventory";

    [JsonPropertyName("randomLootContainers")]
    public required Dictionary<MongoId, RewardDetails> RandomLootContainers { get; set; }

    [JsonPropertyName("sealedAirdropContainer")]
    public required SealedAirdropContainerSettings SealedAirdropContainer { get; set; }

    /// <summary>
    ///     Contains item tpls that the server should consider money and treat the same as roubles/euros/dollars
    /// </summary>
    [JsonPropertyName("customMoneyTpls")]
    public required List<MongoId> CustomMoneyTpls { get; set; }

    /// <summary>
    ///     Multipliers for skill gain when inside menus, NOT in-game
    /// </summary>
    [JsonPropertyName("skillGainMultipliers")]
    public required Dictionary<string, double> SkillGainMultipliers { get; set; }

    /// <summary>
    ///     Container Tpls that should be deprioritised when choosing where to take money from for payments
    /// </summary>
    [JsonPropertyName("deprioritisedMoneyContainers")]
    public required HashSet<MongoId> DeprioritisedMoneyContainers { get; set; }
}

public record RewardDetails
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    [JsonPropertyName("rewardCount")]
    public int RewardCount { get; set; }

    [JsonPropertyName("foundInRaid")]
    public bool FoundInRaid { get; set; }

    [JsonPropertyName("rewardTplPool")]
    public Dictionary<MongoId, double>? RewardTplPool { get; set; }

    [JsonPropertyName("rewardTypePool")]
    public HashSet<MongoId>? RewardTypePool { get; set; }
}

public record SealedAirdropContainerSettings
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("weaponRewardWeight")]
    public required Dictionary<MongoId, double> WeaponRewardWeight { get; set; }

    [JsonPropertyName("defaultPresetsOnly")]
    public bool DefaultPresetsOnly { get; set; }

    /// <summary>
    ///     Should contents be flagged as found in raid when opened
    /// </summary>
    [JsonPropertyName("foundInRaid")]
    public bool FoundInRaid { get; set; }

    [JsonPropertyName("weaponModRewardLimits")]
    public required Dictionary<MongoId, MinMax<int>> WeaponModRewardLimits { get; set; }

    [JsonPropertyName("rewardTypeLimits")]
    public required Dictionary<MongoId, MinMax<int>> RewardTypeLimits { get; set; }

    [JsonPropertyName("ammoBoxWhitelist")]
    public required List<MongoId> AmmoBoxWhitelist { get; set; }

    [JsonPropertyName("allowBossItems")]
    public bool AllowBossItems { get; set; }
}
