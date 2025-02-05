using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Bot;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class BotStaticRouter : StaticRouter
{
    protected static BotCallbacks _botCallbacks;

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
                ) => _botCallbacks.GenerateBots(url, info as GenerateBotsRequestData, sessionID),
                typeof(GenerateBotsRequestData)
            )
        ]
    )
    {
        _botCallbacks = botCallbacks;
    }
}
