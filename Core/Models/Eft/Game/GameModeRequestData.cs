using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class GameModeRequestData
{
    [JsonPropertyName("sessionMode")]
    public string? SessionMode { get; set; }
}