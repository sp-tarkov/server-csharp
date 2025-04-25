using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StressResistance
{
    [JsonPropertyName("HealthNegativeEffect")]
    public double? HealthNegativeEffect
    {
        get;
        set;
    }

    [JsonPropertyName("LowHPDuration")]
    public double? LowHPDuration
    {
        get;
        set;
    }
}
