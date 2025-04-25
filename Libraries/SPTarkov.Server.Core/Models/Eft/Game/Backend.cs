using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record Backend
{
    [JsonPropertyName("Lobby")]
    public string? Lobby
    {
        get;
        set;
    }

    [JsonPropertyName("Trading")]
    public string? Trading
    {
        get;
        set;
    }

    [JsonPropertyName("Messaging")]
    public string? Messaging
    {
        get;
        set;
    }

    [JsonPropertyName("Main")]
    public string? Main
    {
        get;
        set;
    }

    [JsonPropertyName("RagFair")]
    public string? RagFair
    {
        get;
        set;
    }
}
