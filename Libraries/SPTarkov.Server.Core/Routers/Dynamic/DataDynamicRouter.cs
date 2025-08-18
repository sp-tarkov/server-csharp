using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class DataDynamicRouter(JsonUtil jsonUtil, DataCallbacks dataCallbacks)
    : DynamicRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/menu/locale/",
                async (url, info, sessionID, output) => await dataCallbacks.GetLocalesMenu(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/locale/",
                async (url, info, sessionID, output) => await dataCallbacks.GetLocalesGlobal(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/items/prices/",
                async (url, info, sessionID, output) => await dataCallbacks.GetItemPrices(url, info, sessionID)
            ),
        ]
    ) { }
