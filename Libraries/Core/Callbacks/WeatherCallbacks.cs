using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class WeatherCallbacks(
    HttpResponseUtil _httpResponseUtil,
    WeatherController _weatherController
)
{
    /// <summary>
    ///     Handle client/weather
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetWeather(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_weatherController.Generate());
    }

    /// <summary>
    ///     Handle client/localGame/weather
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetLocalWeather(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_weatherController.GenerateLocal(sessionID));
    }
}
