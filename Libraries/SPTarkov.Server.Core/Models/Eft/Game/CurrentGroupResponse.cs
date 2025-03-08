using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record CurrentGroupResponse
{
    [JsonPropertyName("squad")]
    public List<CurrentGroupSquadMember>? Squad
    {
        get;
        set;
    }
}

public record CurrentGroupSquadMember
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("aid")]
    public string? Aid
    {
        get;
        set;
    }

    [JsonPropertyName("info")]
    public CurrentGroupMemberInfo? Info
    {
        get;
        set;
    }

    [JsonPropertyName("isLeader")]
    public bool? IsLeader
    {
        get;
        set;
    }

    [JsonPropertyName("isReady")]
    public bool? IsReady
    {
        get;
        set;
    }
}

public record CurrentGroupMemberInfo
{
    [JsonPropertyName("Nickname")]
    public string? Nickname
    {
        get;
        set;
    }

    [JsonPropertyName("Side")]
    public string? Side
    {
        get;
        set;
    }

    [JsonPropertyName("Level")]
    public string? Level
    {
        get;
        set;
    }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory
    {
        get;
        set;
    }
}
