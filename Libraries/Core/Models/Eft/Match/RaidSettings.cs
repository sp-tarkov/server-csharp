using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Enums.RaidSettings;
using Core.Models.Enums.RaidSettings.TimeAndWeather;

namespace Core.Models.Eft.Match;

public record RaidSettings
{
    [JsonPropertyName("keyId")]
    public string? KeyId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("isLocationTransition")]
    public bool? IsLocationTransition { get; set; }

    [JsonPropertyName("timeVariant")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DateTimeEnum TimeVariant { get; set; }

    [JsonPropertyName("metabolismDisabled")]
    public bool? MetabolismDisabled { get; set; }

    [JsonPropertyName("timeAndWeatherSettings")]
    public TimeAndWeatherSettings? TimeAndWeatherSettings { get; set; }

    [JsonPropertyName("botSettings")]
    public BotSettings? BotSettings { get; set; }

    [JsonPropertyName("wavesSettings")]
    public WavesSettings? WavesSettings { get; set; }

    [JsonPropertyName("side")]
    public SideType? Side { get; set; }

    [JsonPropertyName("raidMode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RaidMode? RaidMode { get; set; }

    [JsonPropertyName("playersSpawnPlace")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlayersSpawnPlace? PlayersSpawnPlace { get; set; }

    [JsonPropertyName("CanShowGroupPreview")]
    public bool? CanShowGroupPreview { get; set; }
}

public record TimeAndWeatherSettings
{
    [JsonPropertyName("isRandomTime")]
    public bool? IsRandomTime { get; set; }

    [JsonPropertyName("isRandomWeather")]
    public bool? IsRandomWeather { get; set; }

    [JsonPropertyName("cloudinessType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CloudinessType? CloudinessType { get; set; }

    [JsonPropertyName("rainType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RainType? RainType { get; set; }

    [JsonPropertyName("fogType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FogType? FogType { get; set; }

    [JsonPropertyName("windType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WindSpeed? WindType { get; set; }

    [JsonPropertyName("timeFlowType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TimeFlowType? TimeFlowType { get; set; }

    [JsonPropertyName("hourOfDay")]
    public int? HourOfDay { get; set; }
}

public record BotSettings
{
    [JsonPropertyName("isScavWars")]
    public bool? IsScavWars { get; set; }

    [JsonPropertyName("botAmount")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BotAmount? BotAmount { get; set; }
}

public record WavesSettings
{
    [JsonPropertyName("botAmount")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BotAmount? BotAmount { get; set; }

    [JsonPropertyName("botDifficulty")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BotDifficulty? BotDifficulty { get; set; }

    [JsonPropertyName("isBosses")]
    public bool? IsBosses { get; set; }

    [JsonPropertyName("isTaggedAndCursed")]
    public bool? IsTaggedAndCursed { get; set; }
}
