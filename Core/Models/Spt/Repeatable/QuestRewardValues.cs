using System.Text.Json.Serialization;

namespace Core.Models.Spt.Repeatable;

public class QuestRewardValues
{
    [JsonPropertyName("skillPointReward")]
    public int? SkillPointReward { get; set; }

    [JsonPropertyName("skillRewardChance")]
    public int? SkillRewardChance { get; set; }

    [JsonPropertyName("rewardReputation")]
    public int? RewardReputation { get; set; }

    [JsonPropertyName("rewardNumItems")]
    public int? RewardNumItems { get; set; }

    [JsonPropertyName("rewardRoubles")]
    public int? RewardRoubles { get; set; }

    [JsonPropertyName("gpCoinRewardCount")]
    public int? GpCoinRewardCount { get; set; }

    [JsonPropertyName("rewardXP")]
    public int? RewardXP { get; set; }
}