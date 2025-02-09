using Core.Callbacks;
using Core.DI;
using Core.Models.Spt.Logging;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
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
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => clientLogCallbacks.ClientLog(url, info as ClientLogRequest, sessionID),
                typeof(ClientLogRequest)
            ),
            new RouteAction(
                "/singleplayer/release",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => clientLogCallbacks.ReleaseNotes()
            ),
            new RouteAction(
                "/singleplayer/enableBSGlogging",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => clientLogCallbacks.BsgLogging()
            )
        ]
    )
    {
    }
}
