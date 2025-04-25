using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdResource
{
    [JsonPropertyName("Value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("UnitsConsumed")]
    public double? UnitsConsumed
    {
        get;
        set;
    }
}
