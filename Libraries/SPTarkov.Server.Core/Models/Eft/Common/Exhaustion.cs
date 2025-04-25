using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Exhaustion
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

    [JsonPropertyName("Damage")]
    public double? Damage
    {
        get;
        set;
    }

    [JsonPropertyName("DamageLoopTime")]
    public double? DamageLoopTime
    {
        get;
        set;
    }
}
