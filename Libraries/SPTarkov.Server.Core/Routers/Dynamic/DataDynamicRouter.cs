using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class DataDynamicRouter : DynamicRouter
{
    public DataDynamicRouter(JsonUtil jsonUtil, DataCallbacks dataCallbacks)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/menu/locale/",
                    (url, info, sessionID, output) =>
                        dataCallbacks.GetLocalesMenu(url, info as EmptyRequestData, sessionID)
                ),
                new RouteAction(
                    "/client/locale/",
                    (url, info, sessionID, output) =>
                        dataCallbacks.GetLocalesGlobal(url, info as EmptyRequestData, sessionID)
                ),
                new RouteAction(
                    "/client/items/prices/",
                    (url, info, sessionID, output) =>
                        dataCallbacks.GetItemPrices(url, info as EmptyRequestData, sessionID)
                ),
            ]
        ) { }
}
