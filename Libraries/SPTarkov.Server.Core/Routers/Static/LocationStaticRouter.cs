using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class LocationStaticRouter(JsonUtil jsonUtil, LocationCallbacks locationCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/locations",
                async (url, info, sessionID, output) => await locationCallbacks.GetLocationData(url, info, sessionID)
            ),
            // For this route it's necessary to not set a specific type for this route
            // As 'sometimes' this route can have the loot request and other times not.
            new RouteAction(
                "/client/airdrop/loot",
                async (url, info, sessionID, output) =>
                    await locationCallbacks.GetAirdropLoot(url, info as GetAirdropLootRequest, sessionID)
            ),
        ]
    ) { }
