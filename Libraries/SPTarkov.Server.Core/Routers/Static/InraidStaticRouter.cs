using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class InraidStaticRouter : StaticRouter
{
    public InraidStaticRouter(InraidCallbacks inraidCallbacks, JsonUtil jsonUtil)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/raid/profile/scavsave",
                    (url, info, sessionID, output) =>
                    {
                        return inraidCallbacks.SaveProgress(
                            url,
                            info as ScavSaveRequestData,
                            sessionID
                        );
                    },
                    typeof(ScavSaveRequestData)
                ),
                new RouteAction(
                    "/singleplayer/settings/raid/menu",
                    (url, info, sessionID, output) =>
                    {
                        return inraidCallbacks.GetRaidMenuSettings();
                    }
                ),
                new RouteAction(
                    "/singleplayer/scav/traitorscavhostile",
                    (url, info, sessionID, output) =>
                    {
                        return inraidCallbacks.GetTraitorScavHostileChance(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
                new RouteAction(
                    "/singleplayer/bosstypes",
                    (url, info, sessionID, output) =>
                    {
                        return inraidCallbacks.GetBossTypes(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
            ]
        ) { }
}
