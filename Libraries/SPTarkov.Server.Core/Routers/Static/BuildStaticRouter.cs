using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Builds;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.PresetBuild;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class BuildStaticRouter(JsonUtil jsonUtil, BuildsCallbacks buildsCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/builds/list",
                async (url, info, sessionID, output) => await buildsCallbacks.GetBuilds(url, info, sessionID)
            ),
            new RouteAction<SetMagazineRequest>(
                "/client/builds/magazine/save",
                async (url, info, sessionID, output) => await buildsCallbacks.CreateMagazineTemplate(url, info, sessionID)
            ),
            new RouteAction<PresetBuildActionRequestData>(
                "/client/builds/weapon/save",
                async (url, info, sessionID, output) => await buildsCallbacks.SetWeapon(url, info, sessionID)
            ),
            new RouteAction<PresetBuildActionRequestData>(
                "/client/builds/equipment/save",
                async (url, info, sessionID, output) => await buildsCallbacks.SetEquipment(url, info, sessionID)
            ),
            new RouteAction<RemoveBuildRequestData>(
                "/client/builds/delete",
                async (url, info, sessionID, output) => await buildsCallbacks.DeleteBuild(url, info, sessionID)
            ),
        ]
    ) { }
