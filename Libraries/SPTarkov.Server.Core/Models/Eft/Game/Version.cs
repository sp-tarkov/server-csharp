using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record Version
{
    [JsonPropertyName("major")]
    public string? Major
    {
        get;
        set;
    }

    [JsonPropertyName("minor")]
    public string? Minor
    {
        get;
        set;
    }

    [JsonPropertyName("game")]
    public string? Game
    {
        get;
        set;
    }

    [JsonPropertyName("backend")]
    public string? Backend
    {
        get;
        set;
    }

    [JsonPropertyName("taxonomy")]
    public string? Taxonomy
    {
        get;
        set;
    }
}
