using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ChronicStaminaFatigue
{
    [JsonPropertyName("EnergyRate")]
    public double? EnergyRate
    {
        get;
        set;
    }

    [JsonPropertyName("WorkingTime")]
    public double? WorkingTime
    {
        get;
        set;
    }

    [JsonPropertyName("TicksEvery")]
    public double? TicksEvery
    {
        get;
        set;
    }

    [JsonPropertyName("EnergyRatePerStack")]
    public double? EnergyRatePerStack
    {
        get;
        set;
    }
}
