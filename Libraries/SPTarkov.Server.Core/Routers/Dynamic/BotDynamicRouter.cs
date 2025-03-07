using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class BotDynamicRouter : DynamicRouter
{
    public BotDynamicRouter(
        JsonUtil jsonUtil,
        BotCallbacks botCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/singleplayer/settings/bot/limit/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => botCallbacks.GetBotLimit(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/difficulty/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => botCallbacks.GetBotDifficulty(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/difficulties",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => botCallbacks.GetAllBotDifficulties(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/maxCap",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => botCallbacks.GetBotCap(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/getBotBehaviours/",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => botCallbacks.GetBotBehaviours()
            )
        ]
    )
    {
    }
}
