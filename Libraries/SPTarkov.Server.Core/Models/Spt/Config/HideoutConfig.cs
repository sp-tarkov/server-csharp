using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record HideoutConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-hideout";

    /// <summary>
    ///     How many seconds should pass before hideout crafts / fuel usage is checked and processed
    /// </summary>
    [JsonPropertyName("runIntervalSeconds")]
    public int RunIntervalSeconds { get; set; }

    /// <summary>
    ///     Default values used to hydrate `RunIntervalSeconds` with
    /// </summary>
    [JsonPropertyName("runIntervalValues")]
    public RunIntervalValues RunIntervalValues { get; set; }

    [JsonPropertyName("hoursForSkillCrafting")]
    public int HoursForSkillCrafting { get; set; }

    [JsonPropertyName("expCraftAmount")]
    public int ExpCraftAmount { get; set; }

    [JsonPropertyName("overrideCraftTimeSeconds")]
    public int OverrideCraftTimeSeconds { get; set; }

    [JsonPropertyName("overrideBuildTimeSeconds")]
    public int OverrideBuildTimeSeconds { get; set; }

    /// <summary>
    ///     Only process a profile's hideout crafts when it has been active in the last x minutes
    /// </summary>
    [JsonPropertyName("updateProfileHideoutWhenActiveWithinMinutes")]
    public int UpdateProfileHideoutWhenActiveWithinMinutes { get; set; }

    [JsonPropertyName("cultistCircle")]
    public CultistCircleSettings CultistCircle { get; set; }

    [JsonPropertyName("hideoutCraftsToAdd")]
    public List<HideoutCraftToAdd> HideoutCraftsToAdd { get; set; }
}

public record HideoutCraftToAdd
{
    /// <summary>
    ///     The new mongoId for the craft to use
    /// </summary>
    [JsonPropertyName("newId")]
    public string NewId { get; set; }

    [JsonPropertyName("requirements")]
    public List<Requirement> Requirements { get; set; }

    [JsonPropertyName("craftIdToCopy")]
    public string CraftIdToCopy { get; set; }

    [JsonPropertyName("craftOutputTpl")]
    public string CraftOutputTpl { get; set; }
}

public record CultistCircleSettings
{
    [JsonPropertyName("maxRewardItemCount")]
    public int MaxRewardItemCount { get; set; }

    [JsonPropertyName("maxAttemptsToPickRewardsWithinBudget")]
    public int MaxAttemptsToPickRewardsWithinBudget { get; set; }

    [JsonPropertyName("rewardPriceMultiplerMinMax")]
    public MinMax<double> RewardPriceMultiplierMinMax { get; set; }

    /// <summary>
    ///     The odds that meeting the highest threshold gives you a bonus to crafting time
    /// </summary>
    [JsonPropertyName("bonusAmountMultiplier")]
    public double BonusAmountMultiplier { get; set; }

    [JsonPropertyName("bonusChanceMultiplier")]
    public double BonusChanceMultiplier { get; set; }

    /// <summary>
    ///     What is considered a "high-value" item
    /// </summary>
    [JsonPropertyName("highValueThresholdRub")]
    public int HighValueThresholdRub { get; set; }

    /// <summary>
    ///     Hideout/task reward crafts have a unique craft time
    /// </summary>
    [JsonPropertyName("hideoutTaskRewardTimeSeconds")]
    public int HideoutTaskRewardTimeSeconds { get; set; }

    /// <summary>
    ///     Rouble amount player needs to sacrifice to get chance of hideout/task rewards
    /// </summary>
    [JsonPropertyName("hideoutCraftSacrificeThresholdRub")]
    public int HideoutCraftSacrificeThresholdRub { get; set; }

    [JsonPropertyName("craftTimeThreshholds")]
    public List<CraftTimeThreshold> CraftTimeThreshholds { get; set; }

    /// <summary>
    ///     -1 means no override, value in seconds
    /// </summary>
    [JsonPropertyName("craftTimeOverride")]
    public int CraftTimeOverride { get; set; }

    /// <summary>
    ///     Specific reward pool when player sacrifices specific item(s)
    /// </summary>
    [JsonPropertyName("directRewards")]
    public List<DirectRewardSettings> DirectRewards { get; set; }

    /// <summary>
    ///     Overrides for reward stack sizes, keyed by item tpl
    /// </summary>
    [JsonPropertyName("directRewardStackSize")]
    public Dictionary<string, MinMax<int>> DirectRewardStackSize { get; set; }

    /// <summary>
    ///     Item tpls to exclude from the reward pool
    /// </summary>
    [JsonPropertyName("rewardItemBlacklist")]
    public List<string> RewardItemBlacklist { get; set; }

    /// <summary>
    ///     Item tpls to include in the reward pool
    /// </summary>
    [JsonPropertyName("additionalRewardItemPool")]
    public List<string> AdditionalRewardItemPool { get; set; }

    [JsonPropertyName("currencyRewards")]
    public Dictionary<string, MinMax<int>> CurrencyRewards { get; set; }
}

public record CraftTimeThreshold : MinMax<int>
{
    [JsonPropertyName("craftTimeSeconds")]
    public int CraftTimeSeconds { get; set; }
}

public record DirectRewardSettings
{
    [JsonPropertyName("reward")]
    public List<string> Reward { get; set; }

    [JsonPropertyName("requiredItems")]
    public List<string> RequiredItems { get; set; }

    [JsonPropertyName("craftTimeSeconds")]
    public int CraftTimeSeconds { get; set; }

    /// <summary>
    ///     Is the reward a one time reward or can it be given multiple times
    /// </summary>
    [JsonPropertyName("repeatable")]
    public bool Repeatable { get; set; }
}
