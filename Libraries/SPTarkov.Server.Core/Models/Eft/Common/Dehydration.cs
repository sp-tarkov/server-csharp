using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Dehydration
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

    [JsonPropertyName("BleedingHealth")]
    public double? BleedingHealth
    {
        get;
        set;
    }

    [JsonPropertyName("BleedingLoopTime")]
    public double? BleedingLoopTime
    {
        get;
        set;
    }

    [JsonPropertyName("BleedingLifeTime")]
    public double? BleedingLifeTime
    {
        get;
        set;
    }

    [JsonPropertyName("DamageOnStrongDehydration")]
    public double? DamageOnStrongDehydration
    {
        get;
        set;
    }

    [JsonPropertyName("StrongDehydrationLoopTime")]
    public double? StrongDehydrationLoopTime
    {
        get;
        set;
    }
}
