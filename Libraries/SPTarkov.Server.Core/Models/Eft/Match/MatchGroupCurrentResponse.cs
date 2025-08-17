using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupCurrentResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("squad")]
    public List<GroupCharacter>? Squad { get; set; }
}
