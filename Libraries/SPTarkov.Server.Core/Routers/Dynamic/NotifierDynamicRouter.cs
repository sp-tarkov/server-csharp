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
                (
                    url,
                    info,
                    sessionID,
                    _
                ) => notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction(
                "/notifierServer",
                (
                    url,
                    info,
                    sessionID,
                    _
                ) => notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction(
                "/push/notifier/get/",
                (
                    url,
                    info,
                    sessionID,
                    _
                ) => notifierCallbacks.GetNotifier(url, info, sessionID)
            ),
            new RouteAction(
                "/push/notifier/get/",
                (
                    url,
                    info,
                    sessionID,
                    _
                ) => notifierCallbacks.GetNotifier(url, info, sessionID)
            )
        ]
    )
    {
    }
}
