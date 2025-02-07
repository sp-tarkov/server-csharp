using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Weather;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Weather;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Controllers;

[Injectable]
public class WeatherController(
    ISptLogger<WeatherController> _logger,
    WeatherGenerator _weatherGenerator,
    SeasonalEventService _seasonalEventService,
    RaidWeatherService _raidWeatherService,
    WeatherHelper _weatherHelper,
    ConfigServer _configServer
)
{
    protected WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();


    /// <summary>
    ///     Handle client/weather
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
    ///     Handle client/localGame/weather
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GetLocalWeatherResponseData GenerateLocal(string sessionId)
    {
        var result = new GetLocalWeatherResponseData
        {
            Season = _seasonalEventService.GetActiveWeatherSeason(),
            Weather = []
        };

        result.Weather.AddRange(_raidWeatherService.GetUpcomingWeather());

        return result;
    }
}
