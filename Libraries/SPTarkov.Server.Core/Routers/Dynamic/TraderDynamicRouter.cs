using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class TraderDynamicRouter(JsonUtil jsonUtil, TraderCallbacks traderCallbacks)
    : DynamicRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/trading/api/getTrader/",
                async (url, info, sessionID, output, cancellationToken) => await traderCallbacks.GetTrader(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/trading/api/getTraderAssort/",
                async (url, info, sessionID, output, cancellationToken) => await traderCallbacks.GetAssort(url, info, sessionID)
            ),
        ]
    ) { }
