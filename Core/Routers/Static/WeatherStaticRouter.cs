using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class WeatherStaticRouter : StaticRouter
{
    protected static WeatherCallbacks _weatherCallbacks;

    public WeatherStaticRouter(
        JsonUtil jsonUtil,
        WeatherCallbacks weatherCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/weather",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _weatherCallbacks.GetWeather(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/localGame/weather",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _weatherCallbacks.GetLocalWeather(url, info as EmptyRequestData, sessionID)),
        ]
    )
    {
        _weatherCallbacks = weatherCallbacks;
    }
}
