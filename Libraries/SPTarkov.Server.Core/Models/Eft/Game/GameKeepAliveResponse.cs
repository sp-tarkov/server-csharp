using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GameKeepAliveResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("msg")]
    public string? Message
    {
        get;
        set;
    }

    [JsonPropertyName("utc_time")]
    public double? UtcTime
    {
        get;
        set;
    }
}
