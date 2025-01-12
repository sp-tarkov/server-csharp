using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Weather;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RaidWeatherService
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;
    private readonly TimeUtil _timeUtil;
    private readonly WeatherGenerator _weatherGenerator;
    private readonly SeasonalEventService _seasonalEventService;
    private readonly WeightedRandomHelper _weightedRandomHelper;
    private readonly ConfigServer _configServer;

    private readonly List<Weather> _weatherForecast = [];

    private readonly WeatherConfig _weatherConfig;

    public RaidWeatherService(
        ILogger logger,
        DatabaseService databaseService,
        TimeUtil timeUtil,
        WeatherGenerator weatherGenerator,
        SeasonalEventService seasonalEventService,
        WeightedRandomHelper weightedRandomHelper,
        ConfigServer configServer)
    {
        _logger = logger;
        _databaseService = databaseService;
        _timeUtil = timeUtil;
        _weatherGenerator = weatherGenerator;
        _seasonalEventService = seasonalEventService;
        _weightedRandomHelper = weightedRandomHelper;
        _configServer = configServer;

        _weatherConfig = _configServer.GetConfig<WeatherConfig>(ConfigTypes.WEATHER);
    }

    /// <summary>
    /// Generate 24 hours of weather data starting from midnight today
    /// </summary>
    public void GenerateWeather(Season currentSeason)
    {
        // When to start generating weather from in milliseconds
        var staringTimestampMs = _timeUtil.GetTodayMidnightTimeStamp();

        // How far into future do we generate weather
        var futureTimestampToReachMs = staringTimestampMs + _timeUtil.GetHoursAsSeconds(_weatherConfig.Weather.GenerateWeatherAmountHours) * 1000; // Convert to milliseconds

        // Keep adding new weather until we have reached desired future date
        var nextTimestampMs = staringTimestampMs;
        while (nextTimestampMs <= futureTimestampToReachMs)
        {
            var newWeatherToAddToCache = _weatherGenerator.GenerateWeather(currentSeason, nextTimestampMs);

            // Add generated weather for time period to cache
            _weatherForecast.Add(newWeatherToAddToCache);

            // Increment timestamp so next loop can begin at correct time
            nextTimestampMs += GetWeightedWeatherTimePeriodMs();
        }
    }

    /// <summary>
    /// Get a time period to increment by, e.g. 15 or 30 minutes as milliseconds
    /// </summary>
    /// <returns>milliseconds</returns>
    protected long GetWeightedWeatherTimePeriodMs()
    {
        //var chosenTimePeriodMinutes = _weightedRandomHelper.WeightedRandom(
        //    _weatherConfig.Weather.TimePeriod.Values,
        //    _weatherConfig.Weather.TimePeriod.Weights).Item;

        //return chosenTimePeriodMinutes * 60 * 1000; // Convert to milliseconds

        throw new NotImplementedException();
    }

    /// <summary>
    /// Find the first matching weather object that applies to the current time
    /// </summary>
    public Weather GetCurrentWeather()
    {
        var currentSeason = _seasonalEventService.GetActiveWeatherSeason();
        ValidateWeatherDataExists(currentSeason);

        return _weatherForecast.Find((weather) => weather.Timestamp >= _timeUtil.GetTimeStamp());
    }

    /// <summary>
    /// Find all matching weather objects that applies to the current time + future
    /// </summary>
    public IEnumerable<Weather> GetUpcomingWeather()
    {
        var currentSeason = _seasonalEventService.GetActiveWeatherSeason();
        ValidateWeatherDataExists(currentSeason);

        return _weatherForecast.Where((weather) => weather.Timestamp >= _timeUtil.GetTimeStamp());
    }

    /// <summary>
    /// Ensure future weather data exists
    /// </summary>
    protected void ValidateWeatherDataExists(Season currentSeason)
    {
        // Clear expired weather data
        _weatherForecast.RemoveAll(weather => weather.Timestamp < _timeUtil.GetTimeStamp());

        // Check data exists for current time
        var result = _weatherForecast.Where((weather) => weather.Timestamp >= _timeUtil.GetTimeStamp());
        if (!result.Any())
        {
            GenerateWeather(currentSeason);
        }

    }
}
