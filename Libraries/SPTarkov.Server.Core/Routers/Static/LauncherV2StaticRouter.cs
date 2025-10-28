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
            new RouteAction<EmptyRequestData>("/launcher/v2/ping", async (_, _, _, _) => await launcherV2Callbacks.Ping()),
            new RouteAction<EmptyRequestData>("/launcher/v2/types", async (_, _, _, _) => await launcherV2Callbacks.Types()),
            new RouteAction<LoginRequestData>("/launcher/v2/login", async (_, info, _, _) => await launcherV2Callbacks.Login(info)),
            new RouteAction<RegisterData>("/launcher/v2/register", async (_, info, _, _) => await launcherV2Callbacks.Register(info)),
            new RouteAction<LoginRequestData>("/launcher/v2/remove", async (_, info, _, _) => await launcherV2Callbacks.Remove(info)),
            new RouteAction<EmptyRequestData>("/launcher/v2/version", async (_, _, _, _) => await launcherV2Callbacks.CompatibleVersion()),
            new RouteAction<EmptyRequestData>("/launcher/v2/mods", async (_, _, _, _) => await launcherV2Callbacks.Mods()),
            new RouteAction<EmptyRequestData>("/launcher/v2/profiles", async (_, _, _, _) => await launcherV2Callbacks.Profiles()),
            new RouteAction<LoginRequestData>("/launcher/v2/profile", async (_, info, _, _) => await launcherV2Callbacks.Profile(info)),
            new RouteAction<RegisterData>("/launcher/v2/wipe", async (_, info, _, _) => await launcherV2Callbacks.Wipe(info)),
        ]
    ) { }
