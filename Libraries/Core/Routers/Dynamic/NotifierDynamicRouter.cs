using Core.Callbacks;
using Core.DI;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
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
