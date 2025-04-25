using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Intoxication
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay
    {
        get;
        set;
    }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime
    {
        get;
        set;
    }

    [JsonPropertyName("DamageHealth")]
    public double? DamageHealth
    {
        get;
        set;
    }

    [JsonPropertyName("HealthLoopTime")]
    public double? HealthLoopTime
    {
        get;
        set;
    }

    [JsonPropertyName("OfflineDurationMin")]
    public double? OfflineDurationMin
    {
        get;
        set;
    }

    [JsonPropertyName("OfflineDurationMax")]
    public double? OfflineDurationMax
    {
        get;
        set;
    }

    [JsonPropertyName("RemovedAfterDeath")]
    public bool? RemovedAfterDeath
    {
        get;
        set;
    }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience
    {
        get;
        set;
    }

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice
    {
        get;
        set;
    }
}
