using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BodyHealthValue
{
    [JsonPropertyName("Value")]
    public double? Value
    {
        get;
        set;
    }
}
