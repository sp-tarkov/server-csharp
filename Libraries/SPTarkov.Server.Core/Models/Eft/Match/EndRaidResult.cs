using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record EndRaidResult
{
    [JsonPropertyName("profile")]
    public PmcData? Profile
    {
        get;
        set;
    }

    /// <summary>
    ///     "Survived/Transit" etc
    /// </summary>
    [JsonPropertyName("result")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExitStatus? Result
    {
        get;
        set;
    }

    [JsonPropertyName("killerId")]
    public string? KillerId
    {
        get;
        set;
    }

    [JsonPropertyName("killerAid")]
    public string? KillerAid
    {
        get;
        set;
    }

    /// <summary>
    ///     "Gate 3" etc
    /// </summary>
    [JsonPropertyName("exitName")]
    public string? ExitName
    {
        get;
        set;
    }

    [JsonPropertyName("inSession")]
    public bool? InSession
    {
        get;
        set;
    }

    [JsonPropertyName("favorite")]
    public bool? Favorite
    {
        get;
        set;
    }

    /// <summary>
    ///     Seconds in raid
    /// </summary>
    [JsonPropertyName("playTime")]
    public double? PlayTime
    {
        get;
        set;
    }
}
