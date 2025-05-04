﻿using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record WeatherConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; } = "spt-weather";

    [JsonPropertyName("acceleration")]
    public double? Acceleration { get; set; }

    [JsonPropertyName("weather")]
    public WeatherValues? Weather { get; set; }

    [JsonPropertyName("seasonDates")]
    public List<SeasonDateTimes>? SeasonDates { get; set; }

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
    [JsonPropertyName("seasonValues")]
    public Dictionary<string, SeasonalValues>? SeasonValues { get; set; }

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
}

public record SeasonalValues
{
    [JsonPropertyName("clouds")]
    public WeatherSettings<double>? Clouds { get; set; }

    [JsonPropertyName("windSpeed")]
    public WeatherSettings<double>? WindSpeed { get; set; }

    [JsonPropertyName("windDirection")]
    public WeatherSettings<WindDirection>? WindDirection { get; set; }

    [JsonPropertyName("windGustiness")]
    public MinMax<double>? WindGustiness { get; set; }

    [JsonPropertyName("rain")]
    public WeatherSettings<double>? Rain { get; set; }

    [JsonPropertyName("rainIntensity")]
    public MinMax<double>? RainIntensity { get; set; }

    [JsonPropertyName("fog")]
    public WeatherSettings<double>? Fog { get; set; }

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
