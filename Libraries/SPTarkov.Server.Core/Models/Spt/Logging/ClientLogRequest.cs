using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Spt.Logging;

public record ClientLogRequest : IRequestData
{
    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    [JsonPropertyName("Level")]
    public LogLevel? Level { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Color")]
    [JsonConverter(typeof(StringToSpectreColorConverter))]
    public Color? Color { get; set; }

    [JsonPropertyName("BackgroundColor")]
    [JsonConverter(typeof(StringToSpectreColorConverter))]
    public Color? BackgroundColor { get; set; }
}
