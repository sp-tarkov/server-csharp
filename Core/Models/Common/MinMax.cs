using System.Text.Json.Serialization;

namespace Core.Models.Common;

public record MinMax
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("max")]
    public double? Max { get; set; }

    [JsonPropertyName("min")]
    public double? Min { get; set; }
}
