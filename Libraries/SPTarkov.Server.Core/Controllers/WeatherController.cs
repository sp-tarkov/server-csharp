using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Weather;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class WeatherController(
    TimeUtil timeUtil,
    WeatherGenerator weatherGenerator,
    SeasonalEventService seasonalEventService,
    RaidWeatherService raidWeatherService,
    WeatherHelper weatherHelper,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Handle client/weather
    /// </summary>
    /// <returns>WeatherData</returns>
    public WeatherData Generate()
    {
        var currentSeason = seasonalEventService.GetActiveWeatherSeason();

        // Prep object to send to client
        var result = new WeatherData
        {
            Acceleration = 0,
            Time = string.Empty,
            Date = string.Empty,
            Weather = null,
            Season = currentSeason,
        };

        // Assign now in a bsg-style formatted string to result object property
        result.Date = timeUtil.GetDateTimeNow().FormatToBsgDate();

        // Get server uptime seconds multiplied by a multiplier and add to current time as seconds
        result.Time = weatherHelper.GetInRaidTime().GetBsgFormattedWeatherTime();
        result.Acceleration = WeatherConfig.Acceleration;

        var presetWeights = cloner.Clone(weatherGenerator.GetWeatherPresetWeightsBySeason(currentSeason));
        result.Weather = weatherGenerator.GenerateWeather(result.Season.Value, ref presetWeights);

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
