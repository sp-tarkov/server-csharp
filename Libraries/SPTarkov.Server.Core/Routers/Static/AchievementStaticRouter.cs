using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await achievementCallbacks.GetAchievements(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/achievement/statistic",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await achievementCallbacks.Statistic(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
