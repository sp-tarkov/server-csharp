using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Ballistic
{
    [JsonPropertyName("GlobalDamageDegradationCoefficient")]
    public double? GlobalDamageDegradationCoefficient
    {
        get;
        set;
    }
}
