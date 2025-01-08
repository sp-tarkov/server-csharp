using System.Text.Json.Serialization;

namespace Core.Models.Spt.Logging;

public class ClientLogRequest
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Level")]
    public LogLevel? Level { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Color")]
    public string? Color { get; set; }

    [JsonPropertyName("BackgroundColor")]
    public string? BackgroundColor { get; set; }
}