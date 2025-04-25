using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RadExposure
{
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
