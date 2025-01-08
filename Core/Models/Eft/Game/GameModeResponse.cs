using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public enum SessionMode
{
    REGULAR,
    PVE
}

public class GameModeResponse
{
    [JsonPropertyName("gameMode")]
    public SessionMode? GameMode { get; set; }

    [JsonPropertyName("backendUrl")]
    public string? BackendUrl { get; set; }
}