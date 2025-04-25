using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Influence
{
    [JsonPropertyName("HealthSlowDownPercentage")]
    public double? HealthSlowDownPercentage
    {
        get;
        set;
    }

    [JsonPropertyName("EnergySlowDownPercentage")]
    public double? EnergySlowDownPercentage
    {
        get;
        set;
    }

    [JsonPropertyName("HydrationSlowDownPercentage")]
    public double? HydrationSlowDownPercentage
    {
        get;
        set;
    }
}
