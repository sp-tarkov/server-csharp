using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record ProfileStatusResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("maxPveCountExceeded")]
    public bool? MaxPveCountExceeded { get; set; }

    [JsonPropertyName("profiles")]
    public List<SessionStatus>? Profiles { get; set; }
}
