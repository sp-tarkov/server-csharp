using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class HealthStaticRouter(JsonUtil jsonUtil, HealthCallbacks healthCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<WorkoutData>(
                "/client/hideout/workout",
                async (url, info, sessionID, output) => await healthCallbacks.HandleWorkoutEffects(url, info, sessionID)
            ),
        ]
    ) { }
