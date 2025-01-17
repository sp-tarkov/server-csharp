using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record GameStartResponse
{
    [JsonPropertyName("utc_time")]
    public double UtcTime { get; set; }
}
