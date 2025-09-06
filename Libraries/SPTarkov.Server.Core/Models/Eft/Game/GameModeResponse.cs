using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public enum SessionMode
{
    Regular,
    Pve,
}

public record GameModeResponse
{
    [JsonPropertyName("gameMode")]
    public string? GameMode { get; set; }

    [JsonPropertyName("backendUrl")]
    public string? BackendUrl { get; set; }
}
