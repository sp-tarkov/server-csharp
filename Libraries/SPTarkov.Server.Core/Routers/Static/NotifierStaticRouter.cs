using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class NotifierStaticRouter(JsonUtil jsonUtil, NotifierCallbacks notifierCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/notifier/channel/create",
                async (url, info, sessionID, output) => await notifierCallbacks.CreateNotifierChannel(url, info, sessionID)
            ),
            new RouteAction<UIDRequestData>(
                "/client/game/profile/select",
                async (url, info, sessionID, output) => await notifierCallbacks.SelectProfile(url, info, sessionID)
            ),
        ]
    ) { }
