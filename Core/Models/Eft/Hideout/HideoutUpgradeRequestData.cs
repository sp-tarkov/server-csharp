using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public record HideoutUpgradeRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutUpgrade";

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
