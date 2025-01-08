using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class MatchGroupStatusResponse
{
    [JsonPropertyName("players")]
    public List<GroupCharacter>? Players { get; set; }

    [JsonPropertyName("maxPveCountExceeded")]
    public bool? MaxPveCountExceeded { get; set; }
}