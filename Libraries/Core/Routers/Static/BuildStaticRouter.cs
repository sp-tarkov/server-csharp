using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Builds;
using Core.Models.Eft.Common;
using Core.Models.Eft.PresetBuild;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
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
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => buildsCallbacks.GetBuilds(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/builds/magazine/save",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => buildsCallbacks.CreateMagazineTemplate(url, info as SetMagazineRequest, sessionID),
                typeof(SetMagazineRequest)
            ),
            new RouteAction(
                "/client/builds/weapon/save",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => buildsCallbacks.SetWeapon(url, info as PresetBuildActionRequestData, sessionID),
                typeof(PresetBuildActionRequestData)
            ),
            new RouteAction(
                "/client/builds/equipment/save",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => buildsCallbacks.SetEquipment(url, info as PresetBuildActionRequestData, sessionID),
                typeof(PresetBuildActionRequestData)
            ),
            new RouteAction(
                "/client/builds/delete",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => buildsCallbacks.DeleteBuild(url, info as RemoveBuildRequestData, sessionID),
                typeof(RemoveBuildRequestData)
            )
        ]
    )
    {
    }
}
