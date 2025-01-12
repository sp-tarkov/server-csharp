using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class TraderStaticRouter : StaticRouter
{
    protected static TraderCallbacks _traderCallbacks;

    public TraderStaticRouter(
        JsonUtil jsonUtil,
        TraderCallbacks traderCallbacks
    ) : base
    (
        jsonUtil,
        [
            new RouteAction(
                "/client/trading/api/traderSettings",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _traderCallbacks.GetTraderSettings(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/singleplayer/moddedTraders",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _traderCallbacks.GetModdedTraderData(url, info as EmptyRequestData, sessionID))
        ]
    )
    {
        _traderCallbacks = traderCallbacks;
    }
}
