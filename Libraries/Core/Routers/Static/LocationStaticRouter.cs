using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Location;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class LocationStaticRouter : StaticRouter
{
    protected static LocationCallbacks _locationCallbacks;

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
                ) => _locationCallbacks.GetLocationData(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/airdrop/loot",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _locationCallbacks.GetAirdropLoot(url, info as GetAirdropLootRequest, sessionID),
                typeof(GetAirdropLootRequest)
            )
        ]
    )
    {
        _locationCallbacks = locationCallbacks;
    }
}
