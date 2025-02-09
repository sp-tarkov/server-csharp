using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class DataDynamicRouter : DynamicRouter
{

    public DataDynamicRouter(
        JsonUtil jsonUtil,
        DataCallbacks dataCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/menu/locale/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => dataCallbacks.GetLocalesMenu(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/locale/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => dataCallbacks.GetLocalesGlobal(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/items/prices/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => dataCallbacks.GetItemPrices(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
