using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MessageSendTimeMultiplier
{
    [JsonPropertyName("multiplier")]
    public double? Multiplier
    {
        get;
        set;
    }
}
