using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Enums;
using Core.Utils.Json.Converters;

namespace Core.Models.Spt.Config;

public record WeatherConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string? Kind
    {
        get;
        set;
    } = "spt-weather";

    [JsonPropertyName("acceleration")]
    public double? Acceleration
    {
        get;
        set;
    }

    [JsonPropertyName("weather")]
    public WeatherValues? Weather
    {
        get;
        set;
    }

    [JsonPropertyName("seasonDates")]
    public List<SeasonDateTimes>? SeasonDates
    {
        get;
        set;
    }

    [JsonPropertyName("overrideSeason")]
    public Season? OverrideSeason
    {
        get;
        set;
    }
}

public record SeasonDateTimes
{
    [JsonPropertyName("seasonType")]
    public Season? SeasonType
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("startDay")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? StartDay
    {
        get;
        set;
    }

    [JsonPropertyName("startMonth")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? StartMonth
    {
        get;
        set;
    }

    [JsonPropertyName("endDay")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? EndDay
    {
        get;
        set;
    }

    [JsonPropertyName("endMonth")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? EndMonth
    {
        get;
        set;
    }
}

public record WeatherValues
{
    [JsonPropertyName("seasonValues")]
    public Dictionary<string, SeasonalValues>? SeasonValues
    {
        get;
        set;
    }

    /**
     * How many hours to generate weather data into the future
     */
    [JsonPropertyName("generateWeatherAmountHours")]
    public int? GenerateWeatherAmountHours
    {
        get;
        set;
    }

    /**
     * Length of each weather period
     */
    [JsonPropertyName("timePeriod")]
    public WeatherSettings<int>? TimePeriod
    {
        get;
        set;
    }
}

public record SeasonalValues
{
    [JsonPropertyName("clouds")]
    public WeatherSettings<double>? Clouds
    {
        get;
        set;
    }

    [JsonPropertyName("windSpeed")]
    public WeatherSettings<double>? WindSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("windDirection")]
    public WeatherSettings<WindDirection>? WindDirection
    {
        get;
        set;
    }

    [JsonPropertyName("windGustiness")]
    public MinMax? WindGustiness
    {
        get;
        set;
    }

    [JsonPropertyName("rain")]
    public WeatherSettings<double>? Rain
    {
        get;
        set;
    }

    [JsonPropertyName("rainIntensity")]
    public MinMax? RainIntensity
    {
        get;
        set;
    }

    [JsonPropertyName("fog")]
    public WeatherSettings<double>? Fog
    {
        get;
        set;
    }

    [JsonPropertyName("temp")]
    public TempDayNight? Temp
    {
        get;
        set;
    }

    [JsonPropertyName("pressure")]
    public MinMax? Pressure
    {
        get;
        set;
    }
}

public record TempDayNight
{
    [JsonPropertyName("day")]
    public MinMax? Day
    {
        get;
        set;
    }

    [JsonPropertyName("night")]
    public MinMax? Night
    {
        get;
        set;
    }
}

public record WeatherSettings<T>
{
    [JsonPropertyName("values")]
    public List<T>? Values
    {
        get;
        set;
    }

    [JsonPropertyName("weights")]
    public List<double>? Weights
    {
        get;
        set;
    }
}
