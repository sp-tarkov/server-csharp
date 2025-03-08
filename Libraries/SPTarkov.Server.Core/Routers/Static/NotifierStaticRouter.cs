using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class NotifierStaticRouter : StaticRouter
{
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
                ) => notifierCallbacks.CreateNotifierChannel(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/profile/select",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => notifierCallbacks.SelectProfile(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            )
        ]
    )
    {
    }
}
