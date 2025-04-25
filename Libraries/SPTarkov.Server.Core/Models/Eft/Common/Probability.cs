using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Probability
{
    [JsonPropertyName("FunctionType")]
    public string? FunctionType
    {
        get;
        set;
    }

    [JsonPropertyName("K")]
    public double? K
    {
        get;
        set;
    }

    [JsonPropertyName("B")]
    public double? B
    {
        get;
        set;
    }

    [JsonPropertyName("Threshold")]
    public double? Threshold
    {
        get;
        set;
    }
}
