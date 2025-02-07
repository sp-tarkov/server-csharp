using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public record MatchGroupCurrentResponse
{
    [JsonPropertyName("squad")]
    public List<GroupCharacter>? Squad
    {
        get;
        set;
    }
}
