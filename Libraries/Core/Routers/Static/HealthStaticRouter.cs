using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Health;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class HealthStaticRouter : StaticRouter
{
    protected static HealthCallbacks _healthCallbacks;

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
                ) => _healthCallbacks.HandleWorkoutEffects(url, info as WorkoutData, sessionID),
                typeof(WorkoutData)
            )
        ]
    )
    {
        _healthCallbacks = healthCallbacks;
    }
}
