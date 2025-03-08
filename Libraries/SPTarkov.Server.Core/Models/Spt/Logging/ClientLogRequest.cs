using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Logging;

public record ClientLogRequest : IRequestData
{
    [JsonPropertyName("Source")]
    public string? Source
    {
        get;
        set;
    }

    [JsonPropertyName("Level")]
    [JsonConverter(typeof(JsonStringEnumConverter))] // TODO: Fix in modules to send enumValue instead of string
    public LogLevel? Level
    {
        get;
        set;
    }

    [JsonPropertyName("Message")]
    public string? Message
    {
        get;
        set;
    }

    [JsonPropertyName("Color")]
    public string? Color
    {
        get;
        set;
    } // TODO: FIX THIS SHIT FOR COLOURS

    [JsonPropertyName("BackgroundColor")]
    public string? BackgroundColor
    {
        get;
        set;
    }
}
