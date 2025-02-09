using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class AchievementStaticRouter : StaticRouter
{
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
                ) => achievementCallbacks.GetAchievements(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/achievement/statistic",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => achievementCallbacks.Statistic(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
