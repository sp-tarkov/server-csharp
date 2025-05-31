using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetLocalesMenu(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/locale/",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetLocalesGlobal(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/items/prices/",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetItemPrices(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
