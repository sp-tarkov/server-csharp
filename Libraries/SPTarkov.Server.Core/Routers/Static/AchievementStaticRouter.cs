using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class AchievementStaticRouter(JsonUtil jsonUtil, AchievementCallbacks achievementCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/achievement/list",
                async (url, info, sessionID, output) => await achievementCallbacks.GetAchievements(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/achievement/statistic",
                async (url, info, sessionID, output) => await achievementCallbacks.Statistic(url, info, sessionID)
            ),
        ]
    ) { }
