using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Weather;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

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
    /// <returns>WeatherData</returns>
    public WeatherData Generate()
    {
        var result = new WeatherData
        {
            Acceleration = 0,
            Time = "",
            Date = "",
            Weather = null,
            Season = Season.AUTUMN,
        };

        _weatherGenerator.CalculateGameTime(result);
        result.Weather = _weatherGenerator.GenerateWeather(result.Season.Value);

        return result;
    }

    /// <summary>
    ///     Handle client/localGame/weather
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>GetLocalWeatherResponseData</returns>
    public GetLocalWeatherResponseData GenerateLocal(string sessionId)
    {
        var result = new GetLocalWeatherResponseData
        {
            Season = _seasonalEventService.GetActiveWeatherSeason(),
            Weather = [],
        };

        result.Weather.AddRange(_raidWeatherService.GetUpcomingWeather());

        return result;
    }
}
