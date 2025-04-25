using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Logging;

public class SptLoggerConfiguration
{
    [JsonPropertyName("loggers")]
    public List<BaseSptLoggerReference> Loggers
    {
        get;
        set;
    }

    [JsonPropertyName("poolingTimeMs")]
    public uint PoolingTimeMs
    {
        get;
        set;
    } = 500;
}
