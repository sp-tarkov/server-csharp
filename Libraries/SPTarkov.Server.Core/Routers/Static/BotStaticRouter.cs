using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class BotStaticRouter : StaticRouter
{
    public BotStaticRouter(
        JsonUtil jsonUtil,
        BotCallbacks botCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/game/bot/generate",
                (
                    url,
                    info,
                    sessionID,
                    outout
                ) => botCallbacks.GenerateBots(url, info as GenerateBotsRequestData, sessionID),
                typeof(GenerateBotsRequestData)
            )
        ]
    )
    {
    }
}
