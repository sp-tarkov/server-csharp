using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.Logger;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Models.Logging;

public abstract class BaseSptLoggerReference
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerType Type
    {
        get;
        set;
    }

    [JsonPropertyName("filters")]
    public List<SptLoggerFilter> Filters
    {
        get;
        set;
    }

    [JsonPropertyName("logLevel")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogLevel LogLevel
    {
        get;
        set;
    }

    [JsonPropertyName("format")]
    public string Format
    {
        get;
        set;
    }
}
