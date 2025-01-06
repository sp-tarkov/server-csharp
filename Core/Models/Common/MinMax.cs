using System.Text.Json.Serialization;

namespace Core.Models.Common;

public class MinMax
{
    [JsonPropertyName("max")]
    public double Max { get; set; }

    [JsonPropertyName("min")]
    public double Min { get; set; }
}