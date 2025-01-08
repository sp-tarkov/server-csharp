using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class RecordShootingRangePoints
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "RecordShootingRangePoints";

    [JsonPropertyName("points")]
    public int? Points { get; set; }
}