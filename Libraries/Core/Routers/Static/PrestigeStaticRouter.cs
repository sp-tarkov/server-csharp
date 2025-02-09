using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Prestige;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

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
