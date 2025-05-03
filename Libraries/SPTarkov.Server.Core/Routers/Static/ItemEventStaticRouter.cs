using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class ItemEventStaticRouter : StaticRouter
{
    public ItemEventStaticRouter(
        JsonUtil jsonUtil,
        ItemEventCallbacks itemEventCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/game/profile/items/moving",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return itemEventCallbacks.HandleEvents(url, info as ItemEventRouterRequest, sessionID); },
                typeof(ItemEventRouterRequest)
            )
        ]
    )
    {
    }
}
