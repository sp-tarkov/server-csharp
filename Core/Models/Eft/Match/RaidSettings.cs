using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Enums.RaidSettings;
using Core.Models.Enums.RaidSettings.TimeAndWeather;

namespace Core.Models.Eft.Match;

public class RaidSettings
{
    [JsonPropertyName("keyId")]
    public string? KeyId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("isLocationTransition")]
    public bool? IsLocationTransition { get; set; }

    [JsonPropertyName("timeVariant")]
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
    public RaidMode? RaidMode { get; set; }

    [JsonPropertyName("playersSpawnPlace")]
    public PlayersSpawnPlace? PlayersSpawnPlace { get; set; }

    [JsonPropertyName("canShowGroupPreview")]
    public bool? CanShowGroupPreview { get; set; }
}

public class TimeAndWeatherSettings
{
    [JsonPropertyName("isRandomTime")]
    public bool? IsRandomTime { get; set; }

    [JsonPropertyName("isRandomWeather")]
    public bool? IsRandomWeather { get; set; }

    [JsonPropertyName("cloudinessType")]
    public CloudinessType? CloudinessType { get; set; }

    [JsonPropertyName("rainType")]
    public RainType? RainType { get; set; }

    [JsonPropertyName("fogType")]
    public FogType? FogType { get; set; }

    [JsonPropertyName("windType")]
    public WindSpeed? WindType { get; set; }

    [JsonPropertyName("timeFlowType")]
    public TimeFlowType? TimeFlowType { get; set; }

    [JsonPropertyName("hourOfDay")]
    public int? HourOfDay { get; set; }
}

public class BotSettings
{
    [JsonPropertyName("isScavWars")]
    public bool? IsScavWars { get; set; }

    [JsonPropertyName("botAmount")]
    public BotAmount? BotAmount { get; set; }
}

public class WavesSettings
{
    [JsonPropertyName("botAmount")]
    public BotAmount? BotAmount { get; set; }

    [JsonPropertyName("botDifficulty")]
    public BotDifficulty? BotDifficulty { get; set; }

    [JsonPropertyName("isBosses")]
    public bool? IsBosses { get; set; }

    [JsonPropertyName("isTaggedAndCursed")]
    public bool? IsTaggedAndCursed { get; set; }
}
