using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutCircleOfCultistProductionStartRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutCircleOfCultistProductionStart";

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}