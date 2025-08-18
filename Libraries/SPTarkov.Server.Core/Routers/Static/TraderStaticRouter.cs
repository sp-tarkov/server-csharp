using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class TraderStaticRouter(JsonUtil jsonUtil, TraderCallbacks traderCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/trading/api/traderSettings",
                async (url, info, sessionID, output) => await traderCallbacks.GetTraderSettings(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/moddedTraders",
                async (url, info, sessionID, output) => await traderCallbacks.GetModdedTraderData(url, info, sessionID)
            ),
        ]
    ) { }
