using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class TraderDynamicRouter : DynamicRouter
{
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
                ) => traderCallbacks.GetTrader(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/trading/api/getTraderAssort/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => traderCallbacks.GetAssort(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
