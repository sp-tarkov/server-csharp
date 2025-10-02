using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class LauncherV2StaticRouter(LauncherV2Callbacks launcherV2Callbacks, JsonUtil jsonUtil)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>("/launcher/v2/ping", async (url, _, sessionID, _) => await launcherV2Callbacks.Ping()),
            new RouteAction<EmptyRequestData>("/launcher/v2/types", async (url, _, sessionID, _) => await launcherV2Callbacks.Types()),
            new RouteAction<LoginRequestData>(
                "/launcher/v2/login",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Login(info)
            ),
            new RouteAction<RegisterData>(
                "/launcher/v2/register",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Register(info)
            ),
            new RouteAction<LoginRequestData>(
                "/launcher/v2/remove",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Remove(info)
            ),
            new RouteAction<EmptyRequestData>(
                "/launcher/v2/version",
                async (url, _, sessionID, _) => await launcherV2Callbacks.CompatibleVersion()
            ),
            new RouteAction<EmptyRequestData>("/launcher/v2/mods", async (url, _, sessionID, _) => await launcherV2Callbacks.Mods()),
            new RouteAction<EmptyRequestData>(
                "/launcher/v2/profiles",
                async (url, _, sessionID, _) => await launcherV2Callbacks.Profiles()
            ),
            new RouteAction<LoginRequestData>(
                "/launcher/v2/profile",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Profile(info)
            ),
        ]
    ) { }
