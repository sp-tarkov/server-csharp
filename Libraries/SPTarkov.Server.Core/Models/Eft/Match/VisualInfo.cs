using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record VisualInfo
{
    [JsonPropertyName("Side")]
    public string? Side
    {
        get;
        set;
    }

    [JsonPropertyName("Level")]
    public int? Level
    {
        get;
        set;
    }

    [JsonPropertyName("Nickname")]
    public string? Nickname
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

    [JsonPropertyName("GameVersion")]
    public string? GameVersion
    {
        get;
        set;
    }
}
