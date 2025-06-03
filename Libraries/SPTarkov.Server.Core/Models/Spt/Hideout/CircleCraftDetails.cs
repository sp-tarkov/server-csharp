using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Models.Spt.Hideout;

public record CircleCraftDetails
{
    [JsonPropertyName("time")]
    public required long Time
    {
        get;
        set;
    }

    [JsonPropertyName("rewardType")]
    public required CircleRewardType RewardType
    {
        get;
        set;
    }

    [JsonPropertyName("rewardAmountRoubles")]
    public required int RewardAmountRoubles
    {
        get;
        set;
    }

    [JsonPropertyName("rewardDetails")]
    public required CraftTimeThreshold RewardDetails
    {
        get;
        set;
    }
}
