using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Metabolism
{
    [JsonPropertyName("HydrationRecoveryRate")]
    public double? HydrationRecoveryRate
    {
        get;
        set;
    }

    [JsonPropertyName("EnergyRecoveryRate")]
    public double? EnergyRecoveryRate
    {
        get;
        set;
    }

    [JsonPropertyName("IncreasePositiveEffectDurationRate")]
    public double? IncreasePositiveEffectDurationRate
    {
        get;
        set;
    }

    [JsonPropertyName("DecreaseNegativeEffectDurationRate")]
    public double? DecreaseNegativeEffectDurationRate
    {
        get;
        set;
    }

    [JsonPropertyName("DecreasePoisonDurationRate")]
    public double? DecreasePoisonDurationRate
    {
        get;
        set;
    }
}
