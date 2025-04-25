using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record HealPrice
{
    [JsonPropertyName("HealthPointPrice")]
    public double? HealthPointPrice
    {
        get;
        set;
    }

    [JsonPropertyName("HydrationPointPrice")]
    public double? HydrationPointPrice
    {
        get;
        set;
    }

    [JsonPropertyName("EnergyPointPrice")]
    public double? EnergyPointPrice
    {
        get;
        set;
    }

    [JsonPropertyName("TrialLevels")]
    public double? TrialLevels
    {
        get;
        set;
    }

    [JsonPropertyName("TrialRaids")]
    public double? TrialRaids
    {
        get;
        set;
    }
}
