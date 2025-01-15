using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Weather;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Generators;

[Injectable]
public class WeatherGenerator
{
    private readonly TimeUtil _timeUtil;
    private readonly SeasonalEventService _seasonalEventService;
    private readonly WeatherHelper _weatherHelper;
    private readonly ConfigServer _configServer;
    private readonly WeightedRandomHelper _weightedRandomHelper;
    private readonly RandomUtil _randomUtil;
    private readonly WeatherConfig _weatherConfig;

    public WeatherGenerator(
    TimeUtil timeUtil,
        SeasonalEventService seasonalEventService,
        WeatherHelper weatherHelper,
        ConfigServer configServer,
        WeightedRandomHelper weightedRandomHelper,
        RandomUtil randomUtil
    )
    {
        _timeUtil = timeUtil;
        _seasonalEventService = seasonalEventService;
        _weatherHelper = weatherHelper;
        _configServer = configServer;
        _weightedRandomHelper = weightedRandomHelper;
        _randomUtil = randomUtil;

        _weatherConfig = _configServer.GetConfig<WeatherConfig>();
    }

    /**
     * Get current + raid datetime and format into correct BSG format and return
     * @param data Weather data
     * @returns WeatherData
     */
    public void CalculateGameTime(WeatherData data)
    {
        var computedDate = new DateTime();
        var formattedDate = this._timeUtil.FormatDate(computedDate);

        data.Date = formattedDate;
        data.Time = GetBsgFormattedInRaidTime();
        data.Acceleration = this._weatherConfig.Acceleration;

        data.Season = this._seasonalEventService.GetActiveWeatherSeason();
    }

    /**
     * Get server uptime seconds multiplied by a multiplier and add to current time as seconds
     * Format to BSGs requirements
     * @param currentDate current date
     * @returns formatted time
     */
    protected string GetBsgFormattedInRaidTime()
    {
        var clientAcceleratedDate = this._weatherHelper.GetInRaidTime();

        return GetBsgFormattedTime(clientAcceleratedDate);
    }

    /**
     * Get current time formatted to fit BSGs requirement
     * @param date date to format into bsg style
     * @returns Time formatted in BSG format
     */
    protected string GetBsgFormattedTime(DateTime date)
    {
        return _timeUtil.FormatTime(date).Replace("-", ":").Replace("-", ":");
    }

    /**
     * Return randomised Weather data with help of config/weather.json
     * @param currentSeason the currently active season
     * @param timestamp OPTIONAL what timestamp to generate the weather data at, defaults to now when not supplied
     * @returns Randomised weather data
     */
    public Weather GenerateWeather(Season currentSeason, long? timestamp = null)
    {
        var weatherValues = GetWeatherValuesBySeason(currentSeason);
        var clouds = GetWeightedClouds(weatherValues);

        // Force rain to off if no clouds
        var rain = clouds <= 0.6 ? 0 : GetWeightedRain(weatherValues);

        // TODO: Ensure Weather settings match Ts Server GetRandomDouble produces a decimal value way higher than ts server
        var result = new Weather
        {
            Pressure = GetRandomDouble(weatherValues.Pressure.Min ?? 0, weatherValues.Pressure.Max ?? 0),
            Temperature = 0,
            Fog = GetWeightedFog(weatherValues),
            RainIntensity =
                rain > 1 ? GetRandomDouble(weatherValues.RainIntensity.Min ?? 0, weatherValues.RainIntensity.Max ?? 0) : 0,
            Rain = rain,
            WindGustiness = GetRandomDouble(weatherValues.WindGustiness.Min ?? 0, weatherValues.WindGustiness.Max ?? 0, 2),
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
        var result = this._weatherConfig.Weather.SeasonValues.TryGetValue(currentSeason.ToString(), out var value);
        if (!result)
        {
            return this._weatherConfig.Weather.SeasonValues["default"];
        }

        return value!;
    }

    /**
     * Choose a temprature for the raid based on time of day
     * @param currentSeason What season tarkov is currently in
     * @param inRaidTimestamp What time is the raid running at
     * @returns Timestamp
     */
    protected double GetRaidTemperature(SeasonalValues weather, long inRaidTimestamp)
    {
        // Convert timestamp to date so we can get current hour and check if its day or night
        var currentRaidTime = new DateTime(inRaidTimestamp);
        var minMax = _weatherHelper.IsHourAtNightTime(currentRaidTime.Hour)
            ? weather.Temp.Night
            : weather.Temp.Day;

        return Math.Round(_randomUtil.GetDouble(minMax.Min ?? 0, minMax.Max ?? 0), 2);
    }

    /**
     * Set Weather date/time/timestamp values to now
     * @param weather Object to update
     * @param timestamp OPTIONAL, define timestamp used
     */
    protected void SetCurrentDateTime(Weather weather, long? timestamp = null)
    {
        var inRaidTime = _weatherHelper.GetInRaidTime(timestamp);
        var normalTime = GetBsgFormattedTime(inRaidTime);
        var formattedDate = _timeUtil.FormatDate(timestamp.HasValue ? _timeUtil.GetDateTimeFromTimeStamp(timestamp.Value) : DateTime.UtcNow);
        var datetimeBsgFormat = $"{formattedDate} {normalTime}";
        
        weather.Timestamp = timestamp ?? _timeUtil.GetTimeStampFromEpoch(inRaidTime); // matches weather.date We use to divide by 1000
        weather.Date = formattedDate; // matches weather.timestamp
        weather.Time = datetimeBsgFormat; // matches weather.timestamp
        weather.SptInRaidTimestamp = _timeUtil.GetTimeStampFromEpoch(inRaidTime);
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
