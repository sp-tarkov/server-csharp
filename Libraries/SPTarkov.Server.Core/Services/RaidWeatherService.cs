using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RaidWeatherService(
    TimeUtil _timeUtil,
    WeatherGenerator _weatherGenerator,
    SeasonalEventService _seasonalEventService,
    WeightedRandomHelper _weightedRandomHelper,
    ConfigServer _configServer
)
{
    protected WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();
    protected List<Weather> _weatherForecast = [];

    /// <summary>
    ///     Generate 24 hours of weather data starting from midnight today
    /// </summary>
    public void GenerateWeather(Season currentSeason)
    {
        // When to start generating weather from in milliseconds
        var staringTimestamp = _timeUtil.GetTodayMidnightTimeStamp();

        // How far into future do we generate weather
        var futureTimestampToReach =
            staringTimestamp + _timeUtil.GetHoursAsSeconds(_weatherConfig.Weather.GenerateWeatherAmountHours ?? 1);

        // Keep adding new weather until we have reached desired future date
        var nextTimestamp = staringTimestamp;
        while (nextTimestamp <= futureTimestampToReach)
        {
            var newWeatherToAddToCache = _weatherGenerator.GenerateWeather(currentSeason, nextTimestamp);

            // Add generated weather for time period to cache
            _weatherForecast.Add(newWeatherToAddToCache);

            // Increment timestamp so next loop can begin at correct time
            nextTimestamp += GetWeightedWeatherTimePeriod();
        }
    }

    /// <summary>
    ///     Get a time period to increment by, e.g. 15 or 30 minutes as milliseconds
    /// </summary>
    /// <returns>milliseconds</returns>
    protected long GetWeightedWeatherTimePeriod()
    {
        var chosenTimePeriodMinutes = _weightedRandomHelper.WeightedRandom(
                _weatherConfig.Weather.TimePeriod.Values,
                _weatherConfig.Weather.TimePeriod.Weights
            )
            .Item;

        return chosenTimePeriodMinutes * 60;
    }

    /// <summary>
    ///     Find the first matching weather object that applies to the current time
    /// </summary>
    public Weather GetCurrentWeather()
    {
        var currentSeason = _seasonalEventService.GetActiveWeatherSeason();
        ValidateWeatherDataExists(currentSeason);

        return _weatherForecast.Find(weather => weather.Timestamp >= _timeUtil.GetTimeStamp());
    }

    /// <summary>
    ///     Find all matching weather objects that applies to the current time + future
    /// </summary>
    public IEnumerable<Weather> GetUpcomingWeather()
    {
        var currentSeason = _seasonalEventService.GetActiveWeatherSeason();
        ValidateWeatherDataExists(currentSeason);

        return _weatherForecast.Where(weather => weather.Timestamp >= _timeUtil.GetTimeStamp());
    }

    /// <summary>
    ///     Ensure future weather data exists
    /// </summary>
    protected void ValidateWeatherDataExists(Season currentSeason)
    {
        // Clear expired weather data
        _weatherForecast.RemoveAll(weather => weather.Timestamp < _timeUtil.GetTimeStamp());

        // Check data exists for current time
        var result = _weatherForecast.Where(weather => weather.Timestamp >= _timeUtil.GetTimeStamp());
        if (!result.Any())
        {
            GenerateWeather(currentSeason);
        }
    }
}
