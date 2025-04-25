using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record PutMetricsRequestData : IRequestData
{
    [JsonPropertyName("sid")]
    public string? SessionId
    {
        get;
        set;
    }

    [JsonPropertyName("Settings")]
    public object? Settings
    {
        get;
        set;
    }

    [JsonPropertyName("SharedSettings")]
    public SharedSettings? SharedSettings
    {
        get;
        set;
    }

    [JsonPropertyName("HardwareDescription")]
    public HardwareDescription? HardwareDescription
    {
        get;
        set;
    }

    [JsonPropertyName("Location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("Metrics")]
    public object? Metrics
    {
        get;
        set;
    }

    [JsonPropertyName("ClientEvents")]
    public ClientEvents? ClientEvents
    {
        get;
        set;
    }

    [JsonPropertyName("SpikeSamples")]
    public List<object>? SpikeSamples
    {
        get;
        set;
    }

    [JsonPropertyName("mode")]
    public string? Mode
    {
        get;
        set;
    }
}
