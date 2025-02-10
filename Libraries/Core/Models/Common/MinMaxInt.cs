using System.Text.Json.Serialization;

namespace Core.Models.Common;

public record MinMaxInt
{
    public MinMaxInt(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public MinMaxInt()
    {
    }

    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }

    [JsonPropertyName("max")]
    public int? Max
    {
        get;
        set;
    }

    [JsonPropertyName("min")]
    public int? Min
    {
        get;
        set;
    }
}
