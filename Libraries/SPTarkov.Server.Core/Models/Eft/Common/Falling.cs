using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Falling
{
    [JsonPropertyName("DamagePerMeter")]
    public double? DamagePerMeter
    {
        get;
        set;
    }

    [JsonPropertyName("SafeHeight")]
    public double? SafeHeight
    {
        get;
        set;
    }
}
