using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Weather;
using Core.Models.Spt.Weather;

namespace Core.Callbacks;

public class WeatherCallbacks
{
    public WeatherCallbacks()
    {
        
    }
    
    /// <summary>
    /// Handle client/weather
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<WeatherData> GetWeather(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/localGame/weather
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<GetLocalWeatherResponseData> GetLocalWeather(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}