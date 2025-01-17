using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Weather;

public record WeatherData
{
    [JsonPropertyName("acceleration")]
    public double? Acceleration { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("weather")]
    public Weather? Weather { get; set; }

    [JsonPropertyName("season")]
    public Season? Season { get; set; }
}

public record Weather
{
    [JsonPropertyName("pressure")]
    public double? Pressure { get; set; }

    [JsonPropertyName("temp")]
    public double? Temperature { get; set; }

    [JsonPropertyName("fog")]
    public double? Fog { get; set; }

    [JsonPropertyName("rain_intensity")]
    public double? RainIntensity { get; set; }

    /** 1 - 3 light rain, 3+ 'rain' */
    [JsonPropertyName("rain")]
    public double? Rain { get; set; }

    [JsonPropertyName("wind_gustiness")]
    public double? WindGustiness { get; set; }

    [JsonPropertyName("wind_direction")]
    public WindDirection? WindDirection { get; set; }

    [JsonPropertyName("wind_speed")]
    public double? WindSpeed { get; set; }

    /** < -0.4 = clear day */
    [JsonPropertyName("cloud")]
    public double? Cloud { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("sptInRaidTimestamp")]
    public long? SptInRaidTimestamp { get; set; }
}
