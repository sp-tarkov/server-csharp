using System.Text.Json.Serialization;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record EndLocalRaidRequestData : IRequestData
{
    /// <summary>
    /// ID of server player just left
    /// </summary>
    [JsonPropertyName("serverId")]
    public string? ServerId { get; set; }

    [JsonPropertyName("results")]
    public EndRaidResult? Results { get; set; }

    /// <summary>
    /// Insured items left in raid by player
    /// </summary>
    [JsonPropertyName("lostInsuredItems")]
    public List<Item>? LostInsuredItems { get; set; }

    /// <summary>
    /// Items sent via traders to player, keyed to service e.g. BTRTransferStash
    /// </summary>
    [JsonPropertyName("transferItems")]
    public Dictionary<string, List<Item>>? TransferItems { get; set; }

    [JsonPropertyName("locationTransit")]
    public LocationTransit? LocationTransit { get; set; }
}

public record EndRaidResult
{
    [JsonPropertyName("profile")]
    public PmcData? Profile { get; set; }

    /// <summary>
    /// "Survived/Transit" etc
    /// </summary>
    [JsonPropertyName("result")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExitStatus? Result { get; set; }

    [JsonPropertyName("killerId")]
    public string? KillerId { get; set; }

    [JsonPropertyName("killerAid")]
    public string? KillerAid { get; set; }

    /// <summary>
    /// "Gate 3" etc
    /// </summary>
    [JsonPropertyName("exitName")]
    public string? ExitName { get; set; }

    [JsonPropertyName("inSession")]
    public bool? InSession { get; set; }

    [JsonPropertyName("favorite")]
    public bool? Favorite { get; set; }

    /// <summary>
    /// Seconds in raid
    /// </summary>
    [JsonPropertyName("playTime")]
    public double? PlayTime { get; set; }
}

public record LocationTransit
{
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("playersCount")]
    public int? PlayersCount { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("profiles")]
    public Dictionary<string, TransitProfile>? Profiles { get; set; }

    [JsonPropertyName("transitionRaidId")]
    public string? TransitionRaidId { get; set; }

    [JsonPropertyName("raidMode")]
    public string? RaidMode { get; set; }

    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("dayTime")]
    public string? DayTime { get; set; }

    /// <summary>
    /// The location player last visited
    /// </summary>
    [JsonPropertyName("sptLastVisitedLocation")]
    public string? SptLastVisitedLocation { get; set; }

    /// <summary>
    /// Name of exit taken
    /// </summary>
    [JsonPropertyName("sptExitName")]
    public string? SptExitName { get; set; }
}

public record TransitProfile
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("keyId")]
    public string? KeyId { get; set; }

    [JsonPropertyName("isSolo")]
    public bool? IsSolo { get; set; }
}
