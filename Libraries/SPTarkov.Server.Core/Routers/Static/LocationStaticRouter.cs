using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class LocationStaticRouter : StaticRouter
{
    public LocationStaticRouter(
        JsonUtil jsonUtil,
        LocationCallbacks locationCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/locations",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => locationCallbacks.GetLocationData(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/airdrop/loot",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => locationCallbacks.GetAirdropLoot(url, info as GetAirdropLootRequest, sessionID),
                typeof(GetAirdropLootRequest)
            )
        ]
    )
    {
    }
}
