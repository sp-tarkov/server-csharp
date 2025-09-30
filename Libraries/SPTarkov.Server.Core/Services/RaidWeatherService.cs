using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RaidWeatherService(
    TimeUtil timeUtil,
    WeatherGenerator weatherGenerator,
    SeasonalEventService seasonalEventService,
    WeightedRandomHelper weightedRandomHelper,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();
    protected readonly List<Weather> WeatherForecast = [];

    /// <summary>
    ///     Generate 24 hours of weather data starting from midnight today
    /// </summary>
    public void GenerateFutureWeatherAndCache(Season currentSeason)
    {
        // When to start generating weather from in milliseconds
        var startingTimestamp = timeUtil.GetTodayMidnightTimeStamp();

        // How far into future do we generate weather
        var futureTimestampToReach = startingTimestamp + timeUtil.GetHoursAsSeconds(WeatherConfig.Weather.GenerateWeatherAmountHours ?? 1);

        // Keep adding new weather until we have reached desired future date
        var nextTimestamp = startingTimestamp;

        // Store this so it can be passed into GenerateWeather()
        WeatherPreset? previousPreset = null;
        var presetWeights = cloner.Clone(weatherGenerator.GetWeatherPresetWeightsBySeason(currentSeason));
        while (nextTimestamp <= futureTimestampToReach)
        {
            // Pass by ref as method will alter weight values
            var newWeatherToAddToCache = weatherGenerator.GenerateWeather(currentSeason, ref presetWeights, nextTimestamp, previousPreset);

            // Add generated weather for time period to cache
            WeatherForecast.Add(newWeatherToAddToCache);

            // Store for use in next loop
            previousPreset = newWeatherToAddToCache.SptChosenPreset;

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
        var chosenTimePeriodMinutes = weightedRandomHelper
            .WeightedRandom(WeatherConfig.Weather.TimePeriod.Values, WeatherConfig.Weather.TimePeriod.Weights)
            .Item;

        return chosenTimePeriodMinutes * 60;
    }

    /// <summary>
    ///     Find the first matching weather object that applies to the current time
    /// </summary>
    public Weather GetCurrentWeather()
    {
        var currentSeason = seasonalEventService.GetActiveWeatherSeason();
        ValidateWeatherDataExists(currentSeason);

        return WeatherForecast.Find(weather => weather.Timestamp >= timeUtil.GetTimeStamp());
    }

    /// <summary>
    ///     Find all matching weather objects that applies to the current time + future
    /// </summary>
    public IEnumerable<Weather> GetUpcomingWeather()
    {
        var currentSeason = seasonalEventService.GetActiveWeatherSeason();
        ValidateWeatherDataExists(currentSeason);

        return WeatherForecast.Where(weather => weather.Timestamp >= timeUtil.GetTimeStamp());
    }

    /// <summary>
    ///     Ensure future weather data exists
    /// </summary>
    protected void ValidateWeatherDataExists(Season currentSeason)
    {
        // Clear expired weather data
        WeatherForecast.RemoveAll(weather => weather.Timestamp < timeUtil.GetTimeStamp());

        // Check data exists for current time
        var result = WeatherForecast.Where(weather => weather.Timestamp >= timeUtil.GetTimeStamp());
        if (!result.Any())
        {
            GenerateFutureWeatherAndCache(currentSeason);
        }
    }
}
