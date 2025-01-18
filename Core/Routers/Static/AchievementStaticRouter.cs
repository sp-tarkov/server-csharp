using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class AchievementStaticRouter : StaticRouter
{
    private static AchievementCallbacks? _achievementCallbacks;

    public AchievementStaticRouter(
        JsonUtil jsonUtil,
        AchievementCallbacks achievementCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/achievement/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _achievementCallbacks?.GetAchievements(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/achievement/statistic",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _achievementCallbacks?.Statistic(url, info as EmptyRequestData, sessionID)),
        ]
    )
    {
        _achievementCallbacks = achievementCallbacks;
    }
}
