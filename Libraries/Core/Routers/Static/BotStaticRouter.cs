using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Bot;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
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
