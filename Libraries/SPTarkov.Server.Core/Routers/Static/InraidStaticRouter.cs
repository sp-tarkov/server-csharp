using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class InraidStaticRouter(InraidCallbacks inRaidCallbacks, JsonUtil jsonUtil)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ScavSaveRequestData>(
                "/raid/profile/scavsave",
                async (url, info, sessionID, output, cancellationToken) => await inRaidCallbacks.SaveProgress(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/raid/menu",
                async (url, info, sessionID, output, cancellationToken) => await inRaidCallbacks.GetRaidMenuSettings()
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/scav/traitorscavhostile",
                async (url, info, sessionID, output, cancellationToken) =>
                    await inRaidCallbacks.GetTraitorScavHostileChance(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/bosstypes",
                async (url, info, sessionID, output, cancellationToken) => await inRaidCallbacks.GetBossTypes(url, info, sessionID)
            ),
        ]
    ) { }
