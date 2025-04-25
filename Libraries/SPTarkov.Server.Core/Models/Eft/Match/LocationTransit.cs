using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record LocationTransit
{
    [JsonPropertyName("hash")]
    public string? Hash
    {
        get;
        set;
    }

    [JsonPropertyName("playersCount")]
    public int? PlayersCount
    {
        get;
        set;
    }

    [JsonPropertyName("ip")]
    public string? Ip
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("profiles")]
    public Dictionary<string, TransitProfile>? Profiles
    {
        get;
        set;
    }

    [JsonPropertyName("transitionRaidId")]
    public string? TransitionRaidId
    {
        get;
        set;
    }

    [JsonPropertyName("raidMode")]
    public string? RaidMode
    {
        get;
        set;
    }

    [JsonPropertyName("side")]
    public string? Side
    {
        get;
        set;
    }

    [JsonPropertyName("dayTime")]
    public string? DayTime
    {
        get;
        set;
    }

    /// <summary>
    ///     The location player last visited
    /// </summary>
    [JsonPropertyName("sptLastVisitedLocation")]
    public string? SptLastVisitedLocation
    {
        get;
        set;
    }

    /// <summary>
    ///     Name of exit taken
    /// </summary>
    [JsonPropertyName("sptExitName")]
    public string? SptExitName
    {
        get;
        set;
    }
}
