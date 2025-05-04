using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class AchievementStaticRouter : StaticRouter
{
    public AchievementStaticRouter(JsonUtil jsonUtil, AchievementCallbacks achievementCallbacks)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/achievement/list",
                    (url, info, sessionID, output) =>
                    {
                        return achievementCallbacks.GetAchievements(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
                new RouteAction(
                    "/client/achievement/statistic",
                    (url, info, sessionID, output) =>
                    {
                        return achievementCallbacks.Statistic(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
            ]
        ) { }
}
