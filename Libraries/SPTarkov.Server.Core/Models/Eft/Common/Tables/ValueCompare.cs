using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ValueCompare
{
    [JsonPropertyName("compareMethod")]
    public string? CompareMethod
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }
}
