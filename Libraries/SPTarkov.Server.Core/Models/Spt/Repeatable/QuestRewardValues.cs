using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Repeatable;

public record QuestRewardValues
{
    [JsonPropertyName("skillPointReward")]
    public double? SkillPointReward { get; set; }

    [JsonPropertyName("skillRewardChance")]
    public double? SkillRewardChance { get; set; }

    [JsonPropertyName("rewardReputation")]
    public double? RewardReputation { get; set; }

    [JsonPropertyName("rewardNumItems")]
    public int? RewardNumItems { get; set; }

    [JsonPropertyName("rewardRoubles")]
    public double? RewardRoubles { get; set; }

    [JsonPropertyName("gpCoinRewardCount")]
    public double? GpCoinRewardCount { get; set; }

    [JsonPropertyName("rewardXP")]
    public double? RewardXP { get; set; }
}
