using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Prestige;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class PrestigeStaticRouter : StaticRouter
{
    public PrestigeStaticRouter(
        JsonUtil jsonUtil,
        PrestigeCallbacks prestigeCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/prestige/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => prestigeCallbacks.GetPrestige(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/prestige/obtain",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => prestigeCallbacks.ObtainPrestige(url, info as ObtainPrestigeRequestList, sessionID),
                typeof(ObtainPrestigeRequestList)
            )
        ]
    )
    {
    }
}
