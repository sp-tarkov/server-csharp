using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class InraidStaticRouter : StaticRouter
{
    public InraidStaticRouter(InraidCallbacks inRaidCallbacks, JsonUtil jsonUtil)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/raid/profile/scavsave",
                    async (url, info, sessionID, output) => await inRaidCallbacks.SaveProgress(url, info as ScavSaveRequestData, sessionID),
                    typeof(ScavSaveRequestData)
                ),
                new RouteAction(
                    "/singleplayer/settings/raid/menu",
                    async (url, info, sessionID, output) => await inRaidCallbacks.GetRaidMenuSettings()
                ),
                new RouteAction(
                    "/singleplayer/scav/traitorscavhostile",
                    async (url, info, sessionID, output) =>
                        await inRaidCallbacks.GetTraitorScavHostileChance(url, info as EmptyRequestData, sessionID)
                ),
                new RouteAction(
                    "/singleplayer/bosstypes",
                    async (url, info, sessionID, output) => await inRaidCallbacks.GetBossTypes(url, info as EmptyRequestData, sessionID)
                ),
            ]
        ) { }
}
