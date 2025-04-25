using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record EventSettings
{
    [JsonPropertyName("EventActive")]
    public bool? EventActive
    {
        get;
        set;
    }

    [JsonPropertyName("EventTime")]
    public double? EventTime
    {
        get;
        set;
    }

    [JsonPropertyName("EventWeather")]
    public EventWeather? EventWeather
    {
        get;
        set;
    }

    [JsonPropertyName("ExitTimeMultiplier")]
    public double? ExitTimeMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("StaminaMultiplier")]
    public double? StaminaMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("SummonFailedWeather")]
    public EventWeather? SummonFailedWeather
    {
        get;
        set;
    }

    [JsonPropertyName("SummonSuccessWeather")]
    public EventWeather? SummonSuccessWeather
    {
        get;
        set;
    }

    [JsonPropertyName("WeatherChangeTime")]
    public double? WeatherChangeTime
    {
        get;
        set;
    }
}
