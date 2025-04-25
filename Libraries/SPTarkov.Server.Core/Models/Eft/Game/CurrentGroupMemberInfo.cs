using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Game;

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
