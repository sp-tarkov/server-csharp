using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GameKeepAliveResponse
{
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
