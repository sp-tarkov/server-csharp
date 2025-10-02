using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class LauncherStaticRouter(LauncherCallbacks launcherCallbacks, JsonUtil jsonUtil)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/launcher/ping",
                async (url, info, sessionID, _) => await launcherCallbacks.Ping(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>("/launcher/server/connect", async (_, _, _, _) => await launcherCallbacks.Connect()),
            new RouteAction<LoginRequestData>(
                "/launcher/profile/login",
                async (url, info, sessionID, _) => await launcherCallbacks.Login(url, info, sessionID)
            ),
            new RouteAction<RegisterData>(
                "/launcher/profile/register",
                async (url, info, sessionID, _) => await launcherCallbacks.Register(url, info, sessionID)
            ),
            new RouteAction<LoginRequestData>(
                "/launcher/profile/get",
                async (url, info, sessionID, _) => await launcherCallbacks.Get(url, info, sessionID)
            ),
            new RouteAction<ChangeRequestData>(
                "/launcher/profile/change/username",
                async (url, info, sessionID, _) => await launcherCallbacks.ChangeUsername(url, info, sessionID)
            ),
            new RouteAction<RegisterData>(
                "/launcher/profile/change/wipe",
                async (url, info, sessionID, _) => await launcherCallbacks.Wipe(url, info, sessionID)
            ),
            new RouteAction<RemoveProfileData>(
                "/launcher/profile/remove",
                async (url, info, sessionID, _) => await launcherCallbacks.RemoveProfile(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/launcher/profile/compatibleTarkovVersion",
                async (_, _, _, _) => await launcherCallbacks.GetCompatibleTarkovVersion()
            ),
            new RouteAction<EmptyRequestData>("/launcher/server/version", async (_, _, _, _) => await launcherCallbacks.GetServerVersion()),
            new RouteAction<EmptyRequestData>(
                "/launcher/server/loadedServerMods",
                async (_, _, _, _) => await launcherCallbacks.GetLoadedServerMods()
            ),
            new RouteAction<EmptyRequestData>(
                "/launcher/server/serverModsUsedByProfile",
                async (url, info, sessionID, _) => await launcherCallbacks.GetServerModsProfileUsed(url, info, sessionID)
            ),
        ]
    ) { }
