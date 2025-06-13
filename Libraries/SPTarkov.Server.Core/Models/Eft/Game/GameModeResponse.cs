using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public enum SessionMode
{
    REGULAR,
    PVE
}

public record GameModeResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

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
