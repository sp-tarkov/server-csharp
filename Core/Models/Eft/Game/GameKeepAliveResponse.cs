using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class GameKeepAliveResponse
{
    [JsonPropertyName("msg")]
    public string? Message { get; set; }

    [JsonPropertyName("utc_time")]
    public double? UtcTime { get; set; }
}