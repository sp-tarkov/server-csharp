using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class WeatherStaticRouter : StaticRouter
{
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
                ) => weatherCallbacks.GetWeather(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/localGame/weather",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => weatherCallbacks.GetLocalWeather(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
