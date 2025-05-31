using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class NotifierDynamicRouter : DynamicRouter
{
    public NotifierDynamicRouter(
        JsonUtil jsonUtil,
        NotifierCallbacks notifierCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/?last_id",
                async (
                    url,
                    info,
                    sessionID,
                    _
                ) => await notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction(
                "/notifierServer",
                async (
                    url,
                    info,
                    sessionID,
                    _
                ) => await notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction(
                "/push/notifier/get/",
                async (
                    url,
                    info,
                    sessionID,
                    _
                ) => await notifierCallbacks.GetNotifier(url, info, sessionID)
            ),
            new RouteAction(
                "/push/notifier/get/",
                async (
                    url,
                    info,
                    sessionID,
                    _
                ) => await notifierCallbacks.GetNotifier(url, info, sessionID)
            )
        ]
    )
    {
    }
}
