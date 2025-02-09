using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Location;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

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
