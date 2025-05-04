using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record MatchGroupCurrentResponse
{
    [JsonPropertyName("squad")]
    public List<GroupCharacter>? Squad { get; set; }
}
