using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BuffSettings
{
    [JsonPropertyName("CommonBuffChanceLevelBonus")]
    public double? CommonBuffChanceLevelBonus
    {
        get;
        set;
    }

    [JsonPropertyName("CommonBuffMinChanceValue")]
    public double? CommonBuffMinChanceValue
    {
        get;
        set;
    }

    [JsonPropertyName("CurrentDurabilityLossToRemoveBuff")]
    public double? CurrentDurabilityLossToRemoveBuff
    {
        get;
        set;
    }

    [JsonPropertyName("MaxDurabilityLossToRemoveBuff")]
    public double? MaxDurabilityLossToRemoveBuff
    {
        get;
        set;
    }

    [JsonPropertyName("RareBuffChanceCoff")]
    public double? RareBuffChanceCoff
    {
        get;
        set;
    }

    [JsonPropertyName("ReceivedDurabilityMaxPercent")]
    public double? ReceivedDurabilityMaxPercent
    {
        get;
        set;
    }
}
