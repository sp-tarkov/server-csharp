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

    private readonly WeatherConfig _weatherConfig;

    public WeatherGenerator(
    TimeUtil timeUtil,
        SeasonalEventService seasonalEventService,
        WeatherHelper weatherHelper,
        ConfigServer configServer)
    {
        _timeUtil = timeUtil;
        _seasonalEventService = seasonalEventService;
        _weatherHelper = weatherHelper;
        _configServer = configServer;

        _weatherConfig = _configServer.GetConfig<WeatherConfig>(ConfigTypes.WEATHER);
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
        throw new NotImplementedException();
    }

    protected SeasonalValues GetWeatherValuesBySeason(Season currentSeason)
    {
        var result = this._weatherConfig.Weather.SeasonValues.TryGetValue(currentSeason.ToString(), out var value);
        if (!result)
        {
            return this._weatherConfig.Weather.SeasonValues["default"];
        }

        return value;
    }

    /**
     * Choose a temprature for the raid based on time of day
     * @param currentSeason What season tarkov is currently in
     * @param inRaidTimestamp What time is the raid running at
     * @returns Timestamp
     */
    protected double GetRaidTemperature(SeasonalValues weather, int inRaidTimestamp)
    {
        throw new NotImplementedException();
    }

    /**
     * Set Weather date/time/timestamp values to now
     * @param weather Object to update
     * @param timestamp OPTIONAL, define timestamp used
     */
    protected void SetCurrentDateTime(Weather weather, int? timestamp = null)
    {
        throw new NotImplementedException();
    }

    protected WindDirection GetWeightedWindDirection(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedClouds(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedWindSpeed(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedFog(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetWeightedRain(SeasonalValues weather)
    {
        throw new NotImplementedException();
    }

    protected double GetRandomFloat(double min, double max, int precision = 3)
    {
        throw new NotImplementedException();
    }
}
