using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Builds;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.PresetBuild;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class BuildStaticRouter : StaticRouter
{
    public BuildStaticRouter(
        JsonUtil jsonUtil,
        BuildsCallbacks buildsCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/builds/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await buildsCallbacks.GetBuilds(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/builds/magazine/save",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await buildsCallbacks.CreateMagazineTemplate(url, info as SetMagazineRequest, sessionID),
                typeof(SetMagazineRequest)
            ),
            new RouteAction(
                "/client/builds/weapon/save",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await buildsCallbacks.SetWeapon(url, info as PresetBuildActionRequestData, sessionID),
                typeof(PresetBuildActionRequestData)
            ),
            new RouteAction(
                "/client/builds/equipment/save",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await buildsCallbacks.SetEquipment(url, info as PresetBuildActionRequestData, sessionID),
                typeof(PresetBuildActionRequestData)
            ),
            new RouteAction(
                "/client/builds/delete",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await buildsCallbacks.DeleteBuild(url, info as RemoveBuildRequestData, sessionID),
                typeof(RemoveBuildRequestData)
            )
        ]
    )
    {
    }
}
