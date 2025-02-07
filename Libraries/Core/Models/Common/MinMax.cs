using System.Text.Json.Serialization;

namespace Core.Models.Common;

public record MinMax
{
    public MinMax(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public MinMax()
    {
    }

    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }

    [JsonPropertyName("max")]
    public double? Max
    {
        get;
        set;
    }

    [JsonPropertyName("min")]
    public double? Min
    {
        get;
        set;
    }
}
