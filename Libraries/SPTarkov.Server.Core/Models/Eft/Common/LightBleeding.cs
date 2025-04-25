using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LightBleeding
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

    [JsonPropertyName("DamageEnergy")]
    public double? DamageEnergy
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

    [JsonPropertyName("EnergyLoopTime")]
    public double? EnergyLoopTime
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

    [JsonPropertyName("DamageHealthDehydrated")]
    public double? DamageHealthDehydrated
    {
        get;
        set;
    }

    [JsonPropertyName("HealthLoopTimeDehydrated")]
    public double? HealthLoopTimeDehydrated
    {
        get;
        set;
    }

    [JsonPropertyName("LifeTimeDehydrated")]
    public double? LifeTimeDehydrated
    {
        get;
        set;
    }

    [JsonPropertyName("EliteVitalityDuration")]
    public double? EliteVitalityDuration
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

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice
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

    [JsonPropertyName("Probability")]
    public Probability? Probability
    {
        get;
        set;
    }
}
