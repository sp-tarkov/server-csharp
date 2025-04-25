using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Color
{
    [JsonPropertyName("r")]
    public double? R
    {
        get;
        set;
    }

    [JsonPropertyName("g")]
    public double? G
    {
        get;
        set;
    }

    [JsonPropertyName("b")]
    public double? B
    {
        get;
        set;
    }

    [JsonPropertyName("a")]
    public double? A
    {
        get;
        set;
    }
}
