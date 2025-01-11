using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Weather;
using Core.Models.Spt.Weather;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class WeatherCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    protected WeatherController _weatherController;

    public WeatherCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        WeatherController weatherController
    )
    {
        _httpResponseUtil = httpResponseUtil;
        _weatherController = weatherController;
    }

    /// <summary>
    /// Handle client/weather
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetWeather(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_weatherController.Generate());
    }

    /// <summary>
    /// Handle client/localGame/weather
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetLocalWeather(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_weatherController.GenerateLocal(sessionID));
    }
}
