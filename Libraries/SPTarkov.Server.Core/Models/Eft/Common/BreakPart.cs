using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BreakPart
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

    [JsonPropertyName("BulletHitProbability")]
    public Probability? BulletHitProbability
    {
        get;
        set;
    }

    [JsonPropertyName("FallingProbability")]
    public Probability? FallingProbability
    {
        get;
        set;
    }
}
