using System.Text.Json.Serialization;
using Core.Models.Common;

namespace Core.Models.Spt.Config;

public record ScavCaseConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-scavcase";

    [JsonPropertyName("rewardItemValueRangeRub")]
    public Dictionary<string, MinMaxDouble> RewardItemValueRangeRub
    {
        get;
        set;
    }

    [JsonPropertyName("moneyRewards")]
    public MoneyRewards MoneyRewards
    {
        get;
        set;
    }

    [JsonPropertyName("ammoRewards")]
    public AmmoRewards AmmoRewards
    {
        get;
        set;
    }

    [JsonPropertyName("rewardItemParentBlacklist")]
    public List<string> RewardItemParentBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("rewardItemBlacklist")]
    public List<string> RewardItemBlacklist
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
    public MoneyLevels RubCount
    {
        get;
        set;
    }

    [JsonPropertyName("usdCount")]
    public MoneyLevels UsdCount
    {
        get;
        set;
    }

    [JsonPropertyName("eurCount")]
    public MoneyLevels EurCount
    {
        get;
        set;
    }

    [JsonPropertyName("gpCount")]
    public MoneyLevels GpCount
    {
        get;
        set;
    }
}

public record MoneyLevels
{
    [JsonPropertyName("common")]
    public MinMaxDouble Common
    {
        get;
        set;
    }

    [JsonPropertyName("rare")]
    public MinMaxDouble Rare
    {
        get;
        set;
    }

    [JsonPropertyName("superrare")]
    public MinMaxDouble SuperRare
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
    public Dictionary<string, List<string>> AmmoRewardBlacklist
    {
        get;
        set;
    }

    [JsonPropertyName("ammoRewardValueRangeRub")]
    public Dictionary<string, MinMaxDouble> AmmoRewardValueRangeRub
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
