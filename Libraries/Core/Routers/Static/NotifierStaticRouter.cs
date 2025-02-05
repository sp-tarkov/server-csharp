using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class NotifierStaticRouter : StaticRouter
{
    protected static NotifierCallbacks _notifierCallbacks;

    public NotifierStaticRouter(
        JsonUtil jsonUtil,
        NotifierCallbacks notifierCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/notifier/channel/create",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _notifierCallbacks.CreateNotifierChannel(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/profile/select",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _notifierCallbacks.SelectProfile(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            )
        ]
    )
    {
        _notifierCallbacks = notifierCallbacks;
    }
}
