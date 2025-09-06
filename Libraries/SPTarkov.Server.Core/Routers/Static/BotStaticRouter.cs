using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class BotStaticRouter(JsonUtil jsonUtil, BotCallbacks botCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<GenerateBotsRequestData>(
                "/client/game/bot/generate",
                async (url, info, sessionID, outout) => await botCallbacks.GenerateBots(url, info, sessionID)
            ),
        ]
    ) { }
