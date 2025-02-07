using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record MatchGroupStartGameRequest : IRequestData
{
    [JsonPropertyName("groupId")]
    public string? GroupId
    {
        get;
        set;
    }

    [JsonPropertyName("servers")]
    public List<Server>? Servers
    {
        get;
        set;
    }
}
