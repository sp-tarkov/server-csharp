using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

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
