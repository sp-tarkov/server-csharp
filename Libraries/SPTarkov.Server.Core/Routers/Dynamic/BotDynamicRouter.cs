using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await botCallbacks.GetBotLimit(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/difficulty/",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await botCallbacks.GetBotDifficulty(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/difficulties",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await botCallbacks.GetAllBotDifficulties(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/maxCap",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await botCallbacks.GetBotCap(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/bot/getBotBehaviours/",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await botCallbacks.GetBotBehaviours()
            )
        ]
    )
    {
    }
}
