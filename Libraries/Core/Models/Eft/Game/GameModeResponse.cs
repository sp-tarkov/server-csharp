using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public enum SessionMode
{
    REGULAR,
    PVE
}

public record GameModeResponse
{
    [JsonPropertyName("gameMode")]
    public string? GameMode
    {
        get;
        set;
    }

    [JsonPropertyName("backendUrl")]
    public string? BackendUrl
    {
        get;
        set;
    }
}
