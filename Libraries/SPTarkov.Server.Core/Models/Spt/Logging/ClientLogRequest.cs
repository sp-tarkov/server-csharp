using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Logging;

public record ClientLogRequest : IRequestData
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Level")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogLevel? Level { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Color")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogTextColor? Color { get; set; }

    [JsonPropertyName("BackgroundColor")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogBackgroundColor? BackgroundColor { get; set; }
}
