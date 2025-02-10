using System.Numerics;
using System.Text.Json.Serialization;

namespace Core.Models.Common;

public record MinMax<T> where T : IMinMaxValue<T>
{
    public MinMax(T min, T max)
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
    public T Max
    {
        get;
        set;
    }

    [JsonPropertyName("min")]
    public T Min
    {
        get;
        set;
    }
}
