using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public record HideoutToggleAreaRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutToggleArea";

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
