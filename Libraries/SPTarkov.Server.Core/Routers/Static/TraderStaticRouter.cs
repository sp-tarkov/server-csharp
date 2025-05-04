using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class TraderStaticRouter : StaticRouter
{
    public TraderStaticRouter(JsonUtil jsonUtil, TraderCallbacks traderCallbacks)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/trading/api/traderSettings",
                    (url, info, sessionID, output) =>
                        traderCallbacks.GetTraderSettings(url, info as EmptyRequestData, sessionID)
                ),
                new RouteAction(
                    "/singleplayer/moddedTraders",
                    (url, info, sessionID, output) =>
                        traderCallbacks.GetModdedTraderData(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        )
                ),
            ]
        ) { }
}
