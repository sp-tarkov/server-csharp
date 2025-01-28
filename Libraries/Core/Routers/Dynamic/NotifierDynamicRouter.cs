using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Utils;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class NotifierDynamicRouter : DynamicRouter
{
    protected static NotifierCallbacks _notifierCallbacks;

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
                    output
                ) => _notifierCallbacks.Notify(url, info, sessionID)
            ),
            new RouteAction(
                "/notifierServer",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _notifierCallbacks.Notify(url, info, sessionID)
            )
        ]
    )
    {
        _notifierCallbacks = notifierCallbacks;
    }
}
