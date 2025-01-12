using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class TraderDynamicRouter : DynamicRouter
{
    protected static TraderCallbacks _traderCallbacks;

    public TraderDynamicRouter(
        JsonUtil jsonUtil,
        TraderCallbacks traderCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/trading/api/getTrader/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _traderCallbacks.GetTrader(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/trading/api/getTraderAssort/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _traderCallbacks.GetAssort(url, info as EmptyRequestData, sessionID)),
        ]
    )
    {
        _traderCallbacks = traderCallbacks;
    }
}
