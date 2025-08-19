using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class WeatherStaticRouter(JsonUtil jsonUtil, WeatherCallbacks weatherCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/weather",
                async (url, info, sessionID, output) => await weatherCallbacks.GetWeather(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/localGame/weather",
                async (url, info, sessionID, output) => await weatherCallbacks.GetLocalWeather(url, info, sessionID)
            ),
        ]
    ) { }
