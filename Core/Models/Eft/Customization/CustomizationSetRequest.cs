using System.Text.Json.Serialization;

namespace Core.Models.Eft.Customization;

public record CustomizationSetRequest
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "CustomizationSet";

    [JsonPropertyName("customizations")]
    public List<CustomizationSetOption>? Customizations { get; set; }
}

public record CustomizationSetOption
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
