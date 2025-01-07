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
    
    public GetBodyResponseData<WeatherData> GetWeather(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetLocalWeatherResponseData> GetLocalWeather(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}