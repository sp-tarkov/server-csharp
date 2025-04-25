using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Wave
{
    [JsonPropertyName("BotPreset")]
    public string? BotPreset
    {
        get;
        set;
    }

    [JsonPropertyName("BotSide")]
    public string? BotSide
    {
        get;
        set;
    }

    [JsonPropertyName("SpawnPoints")]
    public string? SpawnPoints
    {
        get;
        set;
    }

    [JsonPropertyName("WildSpawnType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WildSpawnType? WildSpawnType
    {
        get;
        set;
    }

    [JsonPropertyName("isPlayers")]
    public bool? IsPlayers
    {
        get;
        set;
    }

    [JsonPropertyName("number")]
    public int? Number
    {
        get;
        set;
    }

    [JsonPropertyName("slots_max")]
    public int? SlotsMax
    {
        get;
        set;
    }

    [JsonPropertyName("slots_min")]
    public int? SlotsMin
    {
        get;
        set;
    }

    [JsonPropertyName("time_max")]
    public int? TimeMax
    {
        get;
        set;
    }

    [JsonPropertyName("time_min")]
    public int? TimeMin
    {
        get;
        set;
    }

    /// <summary>
    ///     OPTIONAL - Needs to be unique - Used by custom wave service to ensure same wave isnt added multiple times
    /// </summary>
    [JsonPropertyName("sptId")]
    public string? SptId
    {
        get;
        set;
    }

    [JsonPropertyName("ChanceGroup")]
    public int? ChanceGroup
    {
        get;
        set;
    }

    /// <summary>
    ///     'pve' and/or 'regular'
    /// </summary>
    [JsonPropertyName("SpawnMode")]
    public List<string>? SpawnMode
    {
        get;
        set;
    }

    [JsonPropertyName("OpenZones")]
    public string? OpenZones
    {
        get;
        set;
    }
}
