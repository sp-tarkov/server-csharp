using Core.Annotations;
using Core.Models.Eft.Weather;
using Core.Models.Spt.Weather;

namespace Core.Controllers;

[Injectable]
public class WeatherController
{
    /// <summary>
    /// Handle client/weather
    /// </summary>
    /// <returns></returns>
    public WeatherData Generate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/localGame/weather
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GetLocalWeatherResponseData GenerateLocal(string sessionId)
    {
        throw new NotImplementedException();
    }
}
