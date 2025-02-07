using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record MatchGroupStatusRequest : IRequestData
{
    [JsonPropertyName("location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("savage")]
    public bool? IsSavage
    {
        get;
        set;
    }

    [JsonPropertyName("dt")]
    public string? DateTime
    {
        get;
        set;
    }

    [JsonPropertyName("keyId")]
    public string? KeyId
    {
        get;
        set;
    }

    [JsonPropertyName("raidMode")]
    public RaidMode? RaidMode
    {
        get;
        set;
    }

    [JsonPropertyName("spawnPlace")]
    public string? SpawnPlace
    {
        get;
        set;
    }
}
