using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record ScavCaseConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-scavcase";

    [JsonPropertyName("rewardItemValueRangeRub")]
    public required Dictionary<string, MinMax<double>> RewardItemValueRangeRub
    {
        get;
        set;
    }

    [JsonPropertyName("moneyRewards")]
    public required MoneyRewards MoneyRewards
    {
        get;
        set;
    }

    [JsonPropertyName("ammoRewards")]
    public required AmmoRewards AmmoRewards
    {
        get;
        set;
    }

    [JsonPropertyName("rewardItemParentBlacklist")]
    public required HashSet<string> RewardItemParentBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("rewardItemBlacklist")]
    public required HashSet<string> RewardItemBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("allowMultipleMoneyRewardsPerRarity")]
    public bool AllowMultipleMoneyRewardsPerRarity
    {
        get;
        set;
    }

    [JsonPropertyName("allowMultipleAmmoRewardsPerRarity")]
    public bool AllowMultipleAmmoRewardsPerRarity
    {
        get;
        set;
    }

    [JsonPropertyName("allowBossItemsAsRewards")]
    public bool AllowBossItemsAsRewards
    {
        get;
        set;
    }
}

public record MoneyRewards
{
    [JsonPropertyName("moneyRewardChancePercent")]
    public int MoneyRewardChancePercent
    {
        get;
        set;
    }

    [JsonPropertyName("rubCount")]
    public required MoneyLevels RubCount
    {
        get;
        set;
    }

    [JsonPropertyName("usdCount")]
    public required MoneyLevels UsdCount
    {
        get;
        set;
    }

    [JsonPropertyName("eurCount")]
    public required MoneyLevels EurCount
    {
        get;
        set;
    }

    [JsonPropertyName("gpCount")]
    public required MoneyLevels GpCount
    {
        get;
        set;
    }
}

public record MoneyLevels
{
    [JsonPropertyName("common")]
    public required MinMax<int> Common
    {
        get;
        set;
    }

    [JsonPropertyName("rare")]
    public required MinMax<int> Rare
    {
        get;
        set;
    }

    [JsonPropertyName("superrare")]
    public required MinMax<int> SuperRare
    {
        get;
        set;
    }
}

public record AmmoRewards
{
    [JsonPropertyName("ammoRewardChancePercent")]
    public int AmmoRewardChancePercent
    {
        get;
        set;
    }

    [JsonPropertyName("ammoRewardBlacklist")]
    public required Dictionary<string, List<string>> AmmoRewardBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("ammoRewardValueRangeRub")]
    public required Dictionary<string, MinMax<double>> AmmoRewardValueRangeRub
    {
        get;
        set;
    }

    [JsonPropertyName("minStackSize")]
    public int MinStackSize
    {
        get;
        set;
    }
}
