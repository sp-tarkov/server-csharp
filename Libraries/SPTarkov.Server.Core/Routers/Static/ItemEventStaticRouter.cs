using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class ItemEventStaticRouter(JsonUtil jsonUtil, ItemEventCallbacks itemEventCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ItemEventRouterRequest>(
                "/client/game/profile/items/moving",
                async (url, info, sessionID, output) => await itemEventCallbacks.HandleEvents(url, info, sessionID)
            ),
        ]
    ) { }
