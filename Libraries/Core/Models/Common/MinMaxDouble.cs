using System.Text.Json.Serialization;

namespace Core.Models.Common;

public record MinMaxDouble
{
    public MinMaxDouble(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public MinMaxDouble()
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
