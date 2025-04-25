using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.RaidSettings.TimeAndWeather;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record TimeAndWeatherSettings
{
    [JsonPropertyName("isRandomTime")]
    public bool? IsRandomTime
    {
        get;
        set;
    }

    [JsonPropertyName("isRandomWeather")]
    public bool? IsRandomWeather
    {
        get;
        set;
    }

    [JsonPropertyName("cloudinessType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CloudinessType? CloudinessType
    {
        get;
        set;
    }

    [JsonPropertyName("rainType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RainType? RainType
    {
        get;
        set;
    }

    [JsonPropertyName("fogType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FogType? FogType
    {
        get;
        set;
    }

    [JsonPropertyName("windType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WindSpeed? WindType
    {
        get;
        set;
    }

    [JsonPropertyName("timeFlowType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TimeFlowType? TimeFlowType
    {
        get;
        set;
    }

    [JsonPropertyName("hourOfDay")]
    public int? HourOfDay
    {
        get;
        set;
    }
}
