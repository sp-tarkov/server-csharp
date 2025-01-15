using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Weather;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Weather;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;


namespace Core.Controllers;

[Injectable]
public class WeatherController
{
    protected ISptLogger<WeatherController> _logger;
    protected WeatherGenerator _weatherGenerator;
    protected SeasonalEventService _seasonalEventService;
    protected RaidWeatherService _raidWeatherService;
    protected WeatherHelper _weatherHelper;
    protected ConfigServer _configServer;

    protected WeatherConfig _weatherConfig;

    public WeatherController(
        ISptLogger<WeatherController> logger,
        WeatherGenerator weatherGenerator,
        SeasonalEventService seasonalEventService,
        RaidWeatherService raidWeatherService,
        WeatherHelper weatherHelper,
        ConfigServer configServer
        )
    {
        _logger = logger;
        _weatherGenerator = weatherGenerator;
        _seasonalEventService = seasonalEventService;
        _raidWeatherService = raidWeatherService;
        _weatherHelper = weatherHelper;
        _configServer = configServer;

        _weatherConfig = _configServer.GetConfig<WeatherConfig>();
    }

    /// <summary>
    /// Handle client/weather
    /// </summary>
    /// <returns></returns>
    public WeatherData Generate()
    {
        var result = new WeatherData
        {
            Acceleration = 0,
            Time = "",
            Date = "",
            Weather = null,
            Season = Season.AUTUMN
        };

        _weatherGenerator.CalculateGameTime(result);
        result.Weather = _weatherGenerator.GenerateWeather(result.Season.Value);

        return result;
    }

    /// <summary>
    /// Handle client/localGame/weather
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GetLocalWeatherResponseData GenerateLocal(string sessionId)
    {
        var result = new GetLocalWeatherResponseData()
        {
            Season = (int)_seasonalEventService.GetActiveWeatherSeason(),
            Weather = []
        };

        result.Weather.AddRange(_raidWeatherService.GetUpcomingWeather());

        return result;
    }
}
