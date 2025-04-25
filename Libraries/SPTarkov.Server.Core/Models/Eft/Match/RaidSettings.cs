using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.RaidSettings;
using SPTarkov.Server.Core.Models.Enums.RaidSettings.TimeAndWeather;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record RaidSettings
{
    [JsonPropertyName("keyId")]
    public string? KeyId
    {
        get;
        set;
    }

    [JsonPropertyName("onlinePveRaidStates")]
    public Dictionary<string, bool> OnlinePveRaidStates
    {
        get;
        set;
    } = [];

    [JsonPropertyName("location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("isLocationTransition")]
    public bool? IsLocationTransition
    {
        get;
        set;
    }

    [JsonPropertyName("timeVariant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DateTimeEnum TimeVariant
    {
        get;
        set;
    }

    [JsonPropertyName("metabolismDisabled")]
    public bool? MetabolismDisabled
    {
        get;
        set;
    }

    [JsonPropertyName("timeAndWeatherSettings")]
    public TimeAndWeatherSettings? TimeAndWeatherSettings
    {
        get;
        set;
    }

    [JsonPropertyName("botSettings")]
    public BotSettings? BotSettings
    {
        get;
        set;
    }

    [JsonPropertyName("wavesSettings")]
    public WavesSettings? WavesSettings
    {
        get;
        set;
    }

    [JsonPropertyName("side")]
    public SideType? Side
    {
        get;
        set;
    }

    [JsonPropertyName("raidMode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RaidMode? RaidMode
    {
        get;
        set;
    }

    [JsonPropertyName("playersSpawnPlace")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlayersSpawnPlace? PlayersSpawnPlace
    {
        get;
        set;
    }

    [JsonPropertyName("CanShowGroupPreview")]
    public bool? CanShowGroupPreview
    {
        get;
        set;
    }
}
