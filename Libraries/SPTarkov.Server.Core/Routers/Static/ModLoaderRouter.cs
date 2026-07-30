using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class ModLoaderRouter(JsonUtil jsonUtil, ModLoaderCallbacks modLoaderCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/singleplayer/customEnumEntries",
                async (url, info, sessionID, output, cancellationToken) =>
                    await modLoaderCallbacks.GetCustomEnumEntries(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/moddedTraders",
                async (url, info, sessionID, output, cancellationToken) =>
                    await modLoaderCallbacks.GetCustomizationTraders(url, info, sessionID)
            ),
        ]
    ) { }
