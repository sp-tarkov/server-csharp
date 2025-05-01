using SPTarkov.Common.Annotations;
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
    TimeUtil _timeUtil,
    SeasonalEventService _seasonalEventService,
    WeatherHelper _weatherHelper,
    ConfigServer _configServer,
    WeightedRandomHelper _weightedRandomHelper,
    RandomUtil _randomUtil
)
{
    protected WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Get current + raid datetime and format into correct BSG format.
    /// </summary>
    /// <param name="data"> Weather data </param>
    /// <returns> WeatherData </returns>
    public void CalculateGameTime(WeatherData data)
    {
        var computedDate = _timeUtil.GetDateTimeNow();
        var formattedDate = _timeUtil.FormatDate(computedDate);

        data.Date = formattedDate;
        data.Time = GetBsgFormattedInRaidTime();
        data.Acceleration = _weatherConfig.Acceleration;

        data.Season = _seasonalEventService.GetActiveWeatherSeason();
    }

    /// <summary>
    ///     Get server uptime seconds multiplied by a multiplier and add to current time as seconds.
    ///     Formatted to BSGs requirements
    /// </summary>
    /// <returns>Formatted time as String </returns>
    protected string GetBsgFormattedInRaidTime()
    {
        var clientAcceleratedDate = _weatherHelper.GetInRaidTime();

        return GetBsgFormattedTime(clientAcceleratedDate);
    }

    /// <summary>
    ///     Get current time formatted to fit BSGs requirement
    /// </summary>
    /// <param name="date"> Date to format into bsg style </param>
    /// <returns> Time formatted in BSG format </returns>
    protected string GetBsgFormattedTime(DateTime date)
    {
        return _timeUtil.FormatTime(date).Replace("-", ":").Replace("-", ":");
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
                rain > 1 ? GetRandomDouble(weatherValues.RainIntensity.Min, weatherValues.RainIntensity.Max) : 0,
            Rain = rain,
            WindGustiness = GetRandomDouble(weatherValues.WindGustiness.Min, weatherValues.WindGustiness.Max, 2),
            WindDirection = GetWeightedWindDirection(weatherValues),
            WindSpeed = GetWeightedWindSpeed(weatherValues),
            Cloud = clouds,
            Time = "",
            Date = "",
            Timestamp = 0,
            SptInRaidTimestamp = 0
        };

        SetCurrentDateTime(result, timestamp);

        result.Temperature = GetRaidTemperature(weatherValues, result.SptInRaidTimestamp ?? 0);

        return result;
    }

    protected SeasonalValues GetWeatherValuesBySeason(Season currentSeason)
    {
        var result = _weatherConfig.Weather.SeasonValues.TryGetValue(currentSeason.ToString(), out var value);
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
        var minMax = _weatherHelper.IsHourAtNightTime(currentRaidTime.Hour)
            ? weather.Temp.Night
            : weather.Temp.Day;

        return Math.Round(_randomUtil.GetDouble(minMax.Min, minMax.Max), 2);
    }

    /// <summary>
    ///     Set Weather date/time/timestamp values to now
    /// </summary>
    /// <param name="weather"> Object to update </param>
    /// <param name="timestamp"> Optional, timestamp used </param>
    protected void SetCurrentDateTime(Weather weather, long? timestamp = null)
    {
        var inRaidTime = _weatherHelper.GetInRaidTime(timestamp);
        var normalTime = GetBsgFormattedTime(inRaidTime);
        var formattedDate = _timeUtil.FormatDate(timestamp.HasValue ? _timeUtil.GetDateTimeFromTimeStamp(timestamp.Value) : DateTime.UtcNow);
        var datetimeBsgFormat = $"{formattedDate} {normalTime}";

        weather.Timestamp = timestamp ?? _timeUtil.GetTimeStamp(); // matches weather.date
        weather.Date = formattedDate; // matches weather.timestamp
        weather.Time = datetimeBsgFormat; // matches weather.timestamp
        weather.SptInRaidTimestamp = weather.Timestamp;
    }

    protected WindDirection GetWeightedWindDirection(SeasonalValues weather)
    {
        return _weightedRandomHelper.WeightedRandom(weather.WindDirection.Values, weather.WindDirection.Weights).Item;
    }

    protected double GetWeightedClouds(SeasonalValues weather)
    {
        return _weightedRandomHelper.WeightedRandom(weather.Clouds.Values, weather.Clouds.Weights).Item;
    }

    protected double GetWeightedWindSpeed(SeasonalValues weather)
    {
        return _weightedRandomHelper.WeightedRandom(weather.WindSpeed.Values, weather.WindSpeed.Weights).Item;
    }

    protected double GetWeightedFog(SeasonalValues weather)
    {
        return _weightedRandomHelper.WeightedRandom(weather.Fog.Values, weather.Fog.Weights).Item;
    }

    protected double GetWeightedRain(SeasonalValues weather)
    {
        return _weightedRandomHelper.WeightedRandom(weather.Rain.Values, weather.Rain.Weights).Item;
    }

    protected double GetRandomDouble(double min, double max, int precision = 3)
    {
        return Math.Round(_randomUtil.GetDouble(min, max), precision);
    }
}
