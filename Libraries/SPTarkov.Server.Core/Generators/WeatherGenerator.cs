using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class WeatherGenerator(
    TimeUtil timeUtil,
    SeasonalEventService seasonalEventService,
    WeatherHelper weatherHelper,
    ConfigServer configServer,
    WeightedRandomHelper weightedRandomHelper,
    RandomUtil randomUtil
)
{
    protected readonly WeatherConfig _weatherConfig = configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Get current + raid datetime and format into correct BSG format.
    /// </summary>
    /// <param name="data"> Weather data </param>
    /// <returns> WeatherData </returns>
    public void CalculateGameTime(WeatherData data)
    {
        var computedDate = timeUtil.GetDateTimeNow();
        var formattedDate = computedDate.FormatToBsgDate();

        data.Date = formattedDate;
        data.Time = GetBsgFormattedInRaidTime();
        data.Acceleration = _weatherConfig.Acceleration;

        data.Season = seasonalEventService.GetActiveWeatherSeason();
    }

    /// <summary>
    ///     Get server uptime seconds multiplied by a multiplier and add to current time as seconds.
    ///     Formatted to BSGs requirements
    /// </summary>
    /// <returns>Formatted time as String </returns>
    protected string GetBsgFormattedInRaidTime()
    {
        return weatherHelper.GetInRaidTime().GetBsgFormattedWeatherTime();
    }

    /// <summary>
    ///     Return randomised Weather data with help of config/weather.json
    /// </summary>
    /// <param name="currentSeason"> The currently active season </param>
    /// <param name="timestamp"> Optional, what timestamp to generate the weather data at, defaults to now when not supplied </param>
    /// <returns> Randomised weather data </returns>
    public Weather GenerateWeather(Season currentSeason, long? timestamp = null)
    {
        var weatherValues = GetWeatherValuesBySeason(currentSeason);
        var clouds = GetWeightedClouds(weatherValues);

        // Force rain to off if no clouds
        var rain = clouds <= 0.6 ? 0 : GetWeightedRain(weatherValues);

        // TODO: Ensure Weather settings match Ts Server GetRandomDouble produces a decimal value way higher than ts server
        var result = new Weather
        {
            Pressure = GetRandomDouble(weatherValues.Pressure.Min, weatherValues.Pressure.Max),
            Temperature = 0,
            Fog = GetWeightedFog(weatherValues),
            RainIntensity =
                rain > 1
                    ? GetRandomDouble(
                        weatherValues.RainIntensity.Min,
                        weatherValues.RainIntensity.Max
                    )
                    : 0,
            Rain = rain,
            WindGustiness = GetRandomDouble(
                weatherValues.WindGustiness.Min,
                weatherValues.WindGustiness.Max,
                2
            ),
            WindDirection = GetWeightedWindDirection(weatherValues),
            WindSpeed = GetWeightedWindSpeed(weatherValues),
            Cloud = clouds,
            Time = "",
            Date = "",
            Timestamp = 0,
            SptInRaidTimestamp = 0,
        };

        SetCurrentDateTime(result, timestamp);

        result.Temperature = GetRaidTemperature(weatherValues, result.SptInRaidTimestamp ?? 0);

        return result;
    }

    protected SeasonalValues GetWeatherValuesBySeason(Season currentSeason)
    {
        var result = _weatherConfig.Weather.SeasonValues.TryGetValue(
            currentSeason.ToString(),
            out var value
        );
        if (!result)
        {
            return _weatherConfig.Weather.SeasonValues["default"];
        }

        return value!;
    }

    /// <summary>
    ///     Choose a temperature for the raid based on time of day
    /// </summary>
    /// <param name="weather"> What season Tarkov is currently in </param>
    /// <param name="inRaidTimestamp"> What time is the raid running at </param>
    /// <returns> Timestamp </returns>
    protected double GetRaidTemperature(SeasonalValues weather, long inRaidTimestamp)
    {
        // Convert timestamp to date so we can get current hour and check if its day or night
        var currentRaidTime = new DateTime(inRaidTimestamp);
        var minMax = weatherHelper.IsHourAtNightTime(currentRaidTime.Hour)
            ? weather.Temp.Night
            : weather.Temp.Day;

        return Math.Round(randomUtil.GetDouble(minMax.Min, minMax.Max), 2);
    }

    /// <summary>
    ///     Set Weather date/time/timestamp values to now
    /// </summary>
    /// <param name="weather"> Object to update </param>
    /// <param name="timestamp"> Optional, timestamp used </param>
    protected void SetCurrentDateTime(Weather weather, long? timestamp = null)
    {
        var inRaidTime = timestamp is null
            ? weatherHelper.GetInRaidTime()
            : weatherHelper.GetInRaidTime(timestamp.Value);
        var normalTime = inRaidTime.GetBsgFormattedWeatherTime();
        var formattedDate = (
            timestamp.HasValue
                ? timeUtil.GetDateTimeFromTimeStamp(timestamp.Value)
                : DateTime.UtcNow
        ).FormatToBsgDate();
        var datetimeBsgFormat = $"{formattedDate} {normalTime}";

        weather.Timestamp = timestamp ?? timeUtil.GetTimeStamp(); // matches weather.date
        weather.Date = formattedDate; // matches weather.timestamp
        weather.Time = datetimeBsgFormat; // matches weather.timestamp
        weather.SptInRaidTimestamp = weather.Timestamp;
    }

    protected WindDirection GetWeightedWindDirection(SeasonalValues weather)
    {
        return weightedRandomHelper
            .WeightedRandom(weather.WindDirection.Values, weather.WindDirection.Weights)
            .Item;
    }

    protected double GetWeightedClouds(SeasonalValues weather)
    {
        return weightedRandomHelper
            .WeightedRandom(weather.Clouds.Values, weather.Clouds.Weights)
            .Item;
    }

    protected double GetWeightedWindSpeed(SeasonalValues weather)
    {
        return weightedRandomHelper
            .WeightedRandom(weather.WindSpeed.Values, weather.WindSpeed.Weights)
            .Item;
    }

    protected double GetWeightedFog(SeasonalValues weather)
    {
        return weightedRandomHelper.WeightedRandom(weather.Fog.Values, weather.Fog.Weights).Item;
    }

    protected double GetWeightedRain(SeasonalValues weather)
    {
        return weightedRandomHelper.WeightedRandom(weather.Rain.Values, weather.Rain.Weights).Item;
    }

    protected double GetRandomDouble(double min, double max, int precision = 3)
    {
        return Math.Round(randomUtil.GetDouble(min, max), precision);
    }
}
