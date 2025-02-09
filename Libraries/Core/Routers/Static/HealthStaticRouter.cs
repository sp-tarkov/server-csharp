using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Health;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class HealthStaticRouter : StaticRouter
{
    public HealthStaticRouter(
        JsonUtil jsonUtil,
        HealthCallbacks healthCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/hideout/workout",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => healthCallbacks.HandleWorkoutEffects(url, info as WorkoutData, sessionID),
                typeof(WorkoutData)
            )
        ]
    )
    {
    }
}
