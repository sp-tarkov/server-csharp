using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class ClientLogStaticRouter : StaticRouter
{
    public ClientLogStaticRouter(
        JsonUtil jsonUtil,
        ClientLogCallbacks clientLogCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/singleplayer/log",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await clientLogCallbacks.ClientLog(url, info as ClientLogRequest, sessionID),
                typeof(ClientLogRequest)
            ),
            new RouteAction(
                "/singleplayer/release",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await clientLogCallbacks.ReleaseNotes()
            ),
            new RouteAction(
                "/singleplayer/enableBSGlogging",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await clientLogCallbacks.BsgLogging()
            )
        ]
    )
    {
    }
}
