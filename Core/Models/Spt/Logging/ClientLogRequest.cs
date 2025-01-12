using System.Text.Json.Serialization;
using Core.Models.Logging;
using Core.Models.Utils;

namespace Core.Models.Spt.Logging;

public class ClientLogRequest : IRequestData
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Level")]
    public LogLevel? Level { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Color")]
    public LogTextColor? Color { get; set; }

    [JsonPropertyName("BackgroundColor")]
    public LogBackgroundColor? BackgroundColor { get; set; }
}
