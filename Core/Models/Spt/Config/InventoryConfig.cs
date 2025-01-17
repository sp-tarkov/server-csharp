using System.Text.Json.Serialization;
using Core.Models.Common;

namespace Core.Models.Spt.Config;

public record InventoryConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-inventory";

    /** Should new items purchased by flagged as found in raid */
    [JsonPropertyName("newItemsMarkedFound")]
    public bool NewItemsMarkedFound { get; set; }

    [JsonPropertyName("randomLootContainers")]
    public Dictionary<string, RewardDetails> RandomLootContainers { get; set; }

    [JsonPropertyName("sealedAirdropContainer")]
    public SealedAirdropContainerSettings SealedAirdropContainer { get; set; }

    /** Contains item tpls that the server should consider money and treat the same as roubles/euros/dollars */
    [JsonPropertyName("customMoneyTpls")]
    public List<string> CustomMoneyTpls { get; set; }

    /** Multipliers for skill gain when inside menus, NOT in-game */
    [JsonPropertyName("skillGainMultiplers")]
    public Dictionary<string, double> SkillGainMultipliers { get; set; }

    /** Container Tpls that should be deprioritised when choosing where to take money from for payments */
    [JsonPropertyName("deprioritisedMoneyContainers")]
    public List<string> DeprioritisedMoneyContainers { get; set; }
}

public record RewardDetails
{
    [JsonPropertyName("_type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("rewardCount")]
    public int RewardCount { get; set; }

    [JsonPropertyName("foundInRaid")]
    public bool FoundInRaid { get; set; }

    [JsonPropertyName("rewardTplPool")]
    public Dictionary<string, int>? RewardTplPool { get; set; }

    [JsonPropertyName("rewardTypePool")]
    public List<string>? RewardTypePool { get; set; }
}

public record SealedAirdropContainerSettings
{
    [JsonPropertyName("weaponRewardWeight")]
    public Dictionary<string, double> WeaponRewardWeight { get; set; }

    [JsonPropertyName("defaultPresetsOnly")]
    public bool DefaultPresetsOnly { get; set; }

    /** Should contents be flagged as found in raid when opened */
    [JsonPropertyName("foundInRaid")]
    public bool FoundInRaid { get; set; }

    [JsonPropertyName("weaponModRewardLimits")]
    public Dictionary<string, MinMax> WeaponModRewardLimits { get; set; }

    [JsonPropertyName("rewardTypeLimits")]
    public Dictionary<string, MinMax> RewardTypeLimits { get; set; }

    [JsonPropertyName("ammoBoxWhitelist")]
    public List<string> AmmoBoxWhitelist { get; set; }

    [JsonPropertyName("allowBossItems")]
    public bool AllowBossItems { get; set; }
}
