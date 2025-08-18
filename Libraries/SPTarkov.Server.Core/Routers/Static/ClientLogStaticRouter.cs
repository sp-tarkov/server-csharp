using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class ClientLogStaticRouter(JsonUtil jsonUtil, ClientLogCallbacks clientLogCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ClientLogRequest>(
                "/singleplayer/log",
                async (url, info, sessionID, output) => await clientLogCallbacks.ClientLog(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/release",
                async (url, info, sessionID, output) => await clientLogCallbacks.ReleaseNotes()
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/enableBSGlogging",
                async (url, info, sessionID, output) => await clientLogCallbacks.BsgLogging()
            ),
        ]
    ) { }
