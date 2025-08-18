using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class NotifierDynamicRouter(JsonUtil jsonUtil, NotifierCallbacks notifierCallbacks)
    : DynamicRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/?last_id",
                async (url, info, sessionID, _) => await notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/notifierServer",
                async (url, info, sessionID, _) => await notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/push/notifier/get/",
                async (url, info, sessionID, _) => await notifierCallbacks.GetNotifier(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/push/notifier/get/",
                async (url, info, sessionID, _) => await notifierCallbacks.GetNotifier(url, info, sessionID)
            ),
        ]
    ) { }
