using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.ItemEvent;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class ItemEventStaticRouter : StaticRouter
{
    protected static ItemEventCallbacks _itemEventCallbacks;

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
                ) => _itemEventCallbacks.HandleEvents(url, info as ItemEventRouterRequest, sessionID),
                typeof(ItemEventRouterRequest)
            )
        ]
    )
    {
        _itemEventCallbacks = itemEventCallbacks;
    }
}
