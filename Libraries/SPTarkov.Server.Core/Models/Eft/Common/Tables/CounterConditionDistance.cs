using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record CounterConditionDistance
{
    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("compareMethod")]
    public string? CompareMethod
    {
        get;
        set;
    }
}
