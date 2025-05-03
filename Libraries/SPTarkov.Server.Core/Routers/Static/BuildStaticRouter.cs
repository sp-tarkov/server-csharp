﻿using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Builds;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.PresetBuild;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

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
                ) => { return buildsCallbacks.GetBuilds(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/builds/magazine/save",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return buildsCallbacks.CreateMagazineTemplate(url, info as SetMagazineRequest, sessionID); },
                typeof(SetMagazineRequest)
            ),
            new RouteAction(
                "/client/builds/weapon/save",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return buildsCallbacks.SetWeapon(url, info as PresetBuildActionRequestData, sessionID); },
                typeof(PresetBuildActionRequestData)
            ),
            new RouteAction(
                "/client/builds/equipment/save",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return buildsCallbacks.SetEquipment(url, info as PresetBuildActionRequestData, sessionID); },
                typeof(PresetBuildActionRequestData)
            ),
            new RouteAction(
                "/client/builds/delete",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return buildsCallbacks.DeleteBuild(url, info as RemoveBuildRequestData, sessionID); },
                typeof(RemoveBuildRequestData)
            )
        ]
    )
    {
    }
}
