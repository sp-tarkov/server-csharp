using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class GameStartResponse
{
    [JsonPropertyName("utc_time")]
    public long? UtcTime { get; set; }
}
