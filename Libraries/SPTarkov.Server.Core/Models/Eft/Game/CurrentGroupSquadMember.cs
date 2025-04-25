using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

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
