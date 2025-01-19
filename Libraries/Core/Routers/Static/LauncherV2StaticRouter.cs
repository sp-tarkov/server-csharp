using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Launcher;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class LauncherV2StaticRouter : StaticRouter
{
    public LauncherV2StaticRouter(LauncherV2Callbacks launcherV2Callbacks, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(
                "/launcher/v2/ping",
                (url, _, sessionID, _) => launcherV2Callbacks.Ping()
            ),
            new RouteAction(
                "/launcher/v2/types",
                (url, _, sessionID, _) => launcherV2Callbacks.Types()
            ),
            new RouteAction(
                "/launcher/v2/Login",
                (url, info, sessionID, _) => launcherV2Callbacks.Login(info as LoginRequestData),
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/v2/Register",
                (url, info, sessionID, _) => launcherV2Callbacks.Register(info as RegisterData),
                typeof(RegisterData)
            ),
            new RouteAction(
                "/launcher/v2/passwordChange",
                (url, info, sessionID, _) => launcherV2Callbacks.PasswordChange(info as ChangeRequestData),
                typeof(ChangeRequestData)
            ),
            new RouteAction(
                "/launcher/v2/Remove",
                (url, info, sessionID, _) => launcherV2Callbacks.Remove(info as LoginRequestData),
                    typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/v2/version",
                (url, _, sessionID, _) => launcherV2Callbacks.CompatibleVersion()
            ),
            new RouteAction(
                "/launcher/v2/mods",
                (url, _, sessionID, _) => launcherV2Callbacks.Mods()
            ),
            new RouteAction(
                "/launcher/v2/profiles",
                (url, _, sessionID, _) => launcherV2Callbacks.Profiles()
            ),
            new RouteAction(
                "/launcher/v2/profile",
                (url, _, sessionID, _) => launcherV2Callbacks.Profile()
            ),
            new RouteAction(
                "/launcher/v2/profileMods",
                (url, _, sessionID, _) => launcherV2Callbacks.ProfileMods()
            ),
        ]
    )
    {
    }
}
