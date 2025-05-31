using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class LauncherV2StaticRouter : StaticRouter
{
    public LauncherV2StaticRouter(LauncherV2Callbacks launcherV2Callbacks, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(
                "/launcher/v2/ping",
                async (url, _, sessionID, _) => await launcherV2Callbacks.Ping()
            ),
            new RouteAction(
                "/launcher/v2/types",
                async (url, _, sessionID, _) => await launcherV2Callbacks.Types()
            ),
            new RouteAction(
                "/launcher/v2/login",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Login(info as LoginRequestData),
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/v2/register",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Register(info as RegisterData),
                typeof(RegisterData)
            ),
            new RouteAction(
                "/launcher/v2/passwordChange",
                async (url, info, sessionID, _) => await launcherV2Callbacks.PasswordChange(info as ChangeRequestData),
                typeof(ChangeRequestData)
            ),
            new RouteAction(
                "/launcher/v2/remove",
                async (url, info, sessionID, _) => await launcherV2Callbacks.Remove(info as LoginRequestData),
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/v2/version",
                async (url, _, sessionID, _) => await launcherV2Callbacks.CompatibleVersion()
            ),
            new RouteAction(
                "/launcher/v2/mods",
                async (url, _, sessionID, _) => await launcherV2Callbacks.Mods()
            ),
            new RouteAction(
                "/launcher/v2/profiles",
                async (url, _, sessionID, _) => await launcherV2Callbacks.Profiles()
            ),
            new RouteAction(
                "/launcher/v2/profile",
                async (url, _, sessionID, _) => await launcherV2Callbacks.Profile(sessionID)
            )
        ]
    )
    {
    }
}
