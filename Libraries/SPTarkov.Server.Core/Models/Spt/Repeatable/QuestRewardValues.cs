using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Repeatable;

public record QuestRewardValues
{
    [JsonPropertyName("skillPointReward")]
    public required double SkillPointReward { get; set; }

    [JsonPropertyName("skillRewardChance")]
    public required double SkillRewardChance { get; set; }

    [JsonPropertyName("rewardReputation")]
    public required double RewardReputation { get; set; }

    [JsonPropertyName("rewardNumItems")]
    public required int RewardNumItems { get; set; }

    [JsonPropertyName("rewardRoubles")]
    public required int RewardRoubles { get; set; }

    [JsonPropertyName("gpCoinRewardCount")]
    public required int GpCoinRewardCount { get; set; }

    [JsonPropertyName("rewardXP")]
    public required double RewardXP { get; set; }
}
