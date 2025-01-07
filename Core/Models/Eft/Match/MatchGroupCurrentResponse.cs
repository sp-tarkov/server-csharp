using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class MatchGroupCurrentResponse
{
    [JsonPropertyName("squad")]
    public List<GroupCharacter> Squad { get; set; }
}