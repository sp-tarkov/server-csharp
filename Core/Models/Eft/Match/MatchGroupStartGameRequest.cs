using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class MatchGroupStartGameRequest
{
    [JsonPropertyName("groupId")]
    public string? GroupId { get; set; }

    [JsonPropertyName("servers")]
    public List<Server>? Servers { get; set; }
}