using System.Text.Json.Serialization;
using Types.Models.Spt.Config;

namespace Types.Models.Spt.Hideout;

public class CircleCraftDetails
{
    [JsonPropertyName("time")]
    public int Time { get; set; } // this might not be the right "number" type
    
    [JsonPropertyName("rewardType")]
    public CircleRewardType RewardType { get; set; }
    
    [JsonPropertyName("rewardAmountRoubles")]
    public int RewardAmountRoubles { get; set; }
    
    [JsonPropertyName("rewardDetails")]
    public CraftTimeThreshhold? RewardDetails { get; set; }
}