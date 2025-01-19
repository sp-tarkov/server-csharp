using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public record HandleQTEEventRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    /** true if QTE was successful, otherwise false */
    [JsonPropertyName("results")]
    public List<bool>? Results { get; set; }

    /** Id of the QTE object used from db/hideout/qte.json */
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
