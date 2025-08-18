using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
public class BotDynamicRouter(JsonUtil jsonUtil, BotCallbacks botCallbacks)
    : DynamicRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/bot/limit/",
                async (url, info, sessionID, output) => await botCallbacks.GetBotLimit(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/bot/difficulty/",
                async (url, info, sessionID, output) => await botCallbacks.GetBotDifficulty(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/bot/difficulties",
                async (url, info, sessionID, output) => await botCallbacks.GetAllBotDifficulties(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/bot/maxCap",
                async (url, info, sessionID, output) => await botCallbacks.GetBotCap(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/bot/getBotBehaviours/",
                async (url, info, sessionID, output) => await botCallbacks.GetBotBehaviours()
            ),
        ]
    ) { }
