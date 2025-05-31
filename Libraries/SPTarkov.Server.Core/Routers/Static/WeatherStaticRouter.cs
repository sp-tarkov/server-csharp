using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await weatherCallbacks.GetWeather(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/localGame/weather",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await weatherCallbacks.GetLocalWeather(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
