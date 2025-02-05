using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Spt.Logging;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class ClientLogStaticRouter : StaticRouter
{
    protected static ClientLogCallbacks _clientLogCallbacks;

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
                ) => _clientLogCallbacks.ClientLog(url, info as ClientLogRequest, sessionID),
                typeof(ClientLogRequest)
            ),
            new RouteAction(
                "/singleplayer/release",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _clientLogCallbacks.ReleaseNotes()
            ),
            new RouteAction(
                "/singleplayer/enableBSGlogging",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _clientLogCallbacks.BsgLogging()
            )
        ]
    )
    {
        _clientLogCallbacks = clientLogCallbacks;
    }
}
