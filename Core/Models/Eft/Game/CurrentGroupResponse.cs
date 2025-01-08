using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Game;

public class CurrentGroupResponse
{
    [JsonPropertyName("squad")]
    public List<CurrentGroupSquadMember>? Squad { get; set; }
}

public class CurrentGroupSquadMember
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public string? Aid { get; set; }

    [JsonPropertyName("info")]
    public CurrentGroupMemberInfo? Info { get; set; }

    [JsonPropertyName("isLeader")]
    public bool? IsLeader { get; set; }

    [JsonPropertyName("isReady")]
    public bool? IsReady { get; set; }
}

public class CurrentGroupMemberInfo
{
    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public string? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }
}