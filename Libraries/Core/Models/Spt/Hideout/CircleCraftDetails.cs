using System.Text.Json.Serialization;
using Core.Models.Enums.Hideout;
using Core.Models.Spt.Config;

namespace Core.Models.Spt.Hideout;

public record CircleCraftDetails
{
    [JsonPropertyName("time")]
    public long Time
    {
        get;
        set;
    }

    [JsonPropertyName("rewardType")]
    public CircleRewardType? RewardType
    {
        get;
        set;
    }

    [JsonPropertyName("rewardAmountRoubles")]
    public int? RewardAmountRoubles
    {
        get;
        set;
    }

    [JsonPropertyName("rewardDetails")]
    public CraftTimeThreshold? RewardDetails
    {
        get;
        set;
    }
}
