using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
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
