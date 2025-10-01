using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record WeatherConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind { get; set; } = "spt-weather";

    [JsonPropertyName("acceleration")]
    public double? Acceleration { get; set; }

    [JsonPropertyName("weather")]
    public required WeatherValues Weather { get; set; }

    [JsonPropertyName("seasonDates")]
    public required List<SeasonDateTimes> SeasonDates { get; set; }

    [JsonPropertyName("overrideSeason")]
    public Season? OverrideSeason { get; set; }
}

public record SeasonDateTimes
{
    [JsonPropertyName("seasonType")]
    public Season? SeasonType { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("startDay")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? StartDay { get; set; }

    [JsonPropertyName("startMonth")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? StartMonth { get; set; }

    [JsonPropertyName("endDay")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? EndDay { get; set; }

    [JsonPropertyName("endMonth")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? EndMonth { get; set; }
}

public record WeatherValues
{
    [JsonPropertyName("presetWeights")]
    public Dictionary<string, PresetWeights>? PresetWeights { get; set; }

    /// <summary>
    ///     How many hours to generate weather data into the future
    /// </summary>
    [JsonPropertyName("generateWeatherAmountHours")]
    public int? GenerateWeatherAmountHours { get; set; }

    /// <summary>
    ///     Length of each weather period
    /// </summary>
    [JsonPropertyName("timePeriod")]
    public WeatherSettings<int>? TimePeriod { get; set; }

    [JsonPropertyName("weatherPresetWeight")]
    public Dictionary<string, Dictionary<WeatherPreset, double>> WeatherPresetWeight { get; set; }
}

public enum WeatherPreset
{
    SUNNY = 1,
    RAINY = 2,
    CLOUDY = 3,
}

public record PresetWeights
{
    [JsonPropertyName("clouds")]
    public Dictionary<string, double> Clouds { get; set; }

    [JsonPropertyName("windSpeed")]
    public Dictionary<string, double>? WindSpeed { get; set; }

    [JsonPropertyName("windDirection")]
    public Dictionary<WindDirection, double>? WindDirection { get; set; }

    [JsonPropertyName("windGustiness")]
    public MinMax<double>? WindGustiness { get; set; }

    [JsonPropertyName("rain")]
    public Dictionary<string, double>? Rain { get; set; }

    [JsonPropertyName("rainIntensity")]
    public MinMax<double>? RainIntensity { get; set; }

    [JsonPropertyName("fog")]
    public Dictionary<string, double>? Fog { get; set; }

    [JsonPropertyName("temp")]
    public TempDayNight? Temp { get; set; }

    [JsonPropertyName("pressure")]
    public MinMax<double>? Pressure { get; set; }
}

public record TempDayNight
{
    [JsonPropertyName("day")]
    public MinMax<double>? Day { get; set; }

    [JsonPropertyName("night")]
    public MinMax<double>? Night { get; set; }
}

public record WeatherSettings<T>
{
    [JsonPropertyName("values")]
    public List<T>? Values { get; set; }

    [JsonPropertyName("weights")]
    public List<double>? Weights { get; set; }
}
