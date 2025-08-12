using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Weather;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class WeatherController(
    WeatherGenerator weatherGenerator,
    SeasonalEventService seasonalEventService,
    RaidWeatherService raidWeatherService,
    ConfigServer configServer
)
{
    protected WeatherConfig _weatherConfig = configServer.GetConfig<WeatherConfig>();

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

        weatherGenerator.CalculateGameTime(result);
        result.Weather = weatherGenerator.GenerateWeather(result.Season.Value);

        return result;
    }

    /// <summary>
    ///     Handle client/localGame/weather
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>GetLocalWeatherResponseData</returns>
    public GetLocalWeatherResponseData GenerateLocal(MongoId sessionId)
    {
        var result = new GetLocalWeatherResponseData { Season = seasonalEventService.GetActiveWeatherSeason(), Weather = [] };

        result.Weather.AddRange(raidWeatherService.GetUpcomingWeather());

        return result;
    }
}
