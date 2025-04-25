using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record Customization
{
    [JsonPropertyName("Head")]
    public string? Head
    {
        get;
        set;
    }

    [JsonPropertyName("Body")]
    public string? Body
    {
        get;
        set;
    }

    [JsonPropertyName("Feet")]
    public string? Feet
    {
        get;
        set;
    }

    [JsonPropertyName("Hands")]
    public string? Hands
    {
        get;
        set;
    }
}
