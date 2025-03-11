using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record InventoryConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-inventory";

    /// <summary>
    /// Should new items purchased by flagged as found in raid
    /// </summary>
    [JsonPropertyName("newItemsMarkedFound")]
    public bool NewItemsMarkedFound
    {
        get;
        set;
    }

    [JsonPropertyName("randomLootContainers")]
    public Dictionary<string, RewardDetails> RandomLootContainers
    {
        get;
        set;
    }

    [JsonPropertyName("sealedAirdropContainer")]
    public SealedAirdropContainerSettings SealedAirdropContainer
    {
        get;
        set;
    }

    /// <summary>
    /// Contains item tpls that the server should consider money and treat the same as roubles/euros/dollars
    /// </summary>
    [JsonPropertyName("customMoneyTpls")]
    public List<string> CustomMoneyTpls
    {
        get;
        set;
    }

    /// <summary>
    /// Multipliers for skill gain when inside menus, NOT in-game
    /// </summary>
    [JsonPropertyName("skillGainMultiplers")]
    public Dictionary<string, double> SkillGainMultipliers
    {
        get;
        set;
    }

    /// <summary>
    /// Container Tpls that should be deprioritised when choosing where to take money from for payments
    /// </summary>
    [JsonPropertyName("deprioritisedMoneyContainers")]
    public HashSet<string> DeprioritisedMoneyContainers
    {
        get;
        set;
    }
}

public record RewardDetails
{
    [JsonPropertyName("_type")]
    public string? Type
    {
        get;
        set;
    }

    [JsonPropertyName("rewardCount")]
    public int RewardCount
    {
        get;
        set;
    }

    [JsonPropertyName("foundInRaid")]
    public bool FoundInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("rewardTplPool")]
    public Dictionary<MongoId, double>? RewardTplPool
    {
        get;
        set;
    }

    [JsonPropertyName("rewardTypePool")]
    public List<MongoId>? RewardTypePool
    {
        get;
        set;
    }
}

public record SealedAirdropContainerSettings
{
    [JsonPropertyName("weaponRewardWeight")]
    public Dictionary<MongoId, double> WeaponRewardWeight
    {
        get;
        set;
    }

    [JsonPropertyName("defaultPresetsOnly")]
    public bool DefaultPresetsOnly
    {
        get;
        set;
    }

    /// <summary>
    /// Should contents be flagged as found in raid when opened
    /// </summary>
    [JsonPropertyName("foundInRaid")]
    public bool FoundInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("weaponModRewardLimits")]
    public Dictionary<MongoId, MinMax<int>> WeaponModRewardLimits
    {
        get;
        set;
    }

    [JsonPropertyName("rewardTypeLimits")]
    public Dictionary<MongoId, MinMax<int>> RewardTypeLimits
    {
        get;
        set;
    }

    [JsonPropertyName("ammoBoxWhitelist")]
    public List<MongoId> AmmoBoxWhitelist
    {
        get;
        set;
    }

    [JsonPropertyName("allowBossItems")]
    public bool AllowBossItems
    {
        get;
        set;
    }
}
