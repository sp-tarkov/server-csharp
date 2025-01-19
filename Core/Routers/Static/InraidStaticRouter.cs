using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.InRaid;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class InraidStaticRouter : StaticRouter
{
    public InraidStaticRouter(InraidCallbacks inraidCallbacks, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(
                "/raid/profile/scavsave",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => inraidCallbacks.SaveProgress(url, info as ScavSaveRequestData, sessionID),
                typeof(ScavSaveRequestData)),
            new RouteAction(
                "/singleplayer/settings/raid/menu",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => inraidCallbacks.GetRaidMenuSettings()),
            new RouteAction(
                "/singleplayer/scav/traitorscavhostile",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => inraidCallbacks.GetTraitorScavHostileChance(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/singleplayer/bossconvert",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => inraidCallbacks.GetBossConvertSettings(url, info as EmptyRequestData, sessionID)),
        ])
    {
    }
}
