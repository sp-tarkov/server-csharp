using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class LauncherStaticRouter : StaticRouter
{
    public LauncherStaticRouter(LauncherCallbacks launcherCallbacks, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(
                "/launcher/ping",
                (url, _, sessionID, _) => { return launcherCallbacks.Ping(url, null, sessionID); }),
            new RouteAction(
                "/launcher/server/connect",
                (_, _, _, _) => { return launcherCallbacks.Connect(); }),
            new RouteAction(
                "/launcher/profile/login",
                (url, info, sessionID, _) => { return launcherCallbacks.Login(url, info as LoginRequestData, sessionID); },
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/profile/register",
                (url, info, sessionID, _) => { return launcherCallbacks.Register(url, info as RegisterData, sessionID); },
                typeof(RegisterData)
            ),
            new RouteAction(
                "/launcher/profile/get",
                (url, info, sessionID, _) => { return launcherCallbacks.Get(url, info as LoginRequestData, sessionID); },
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/profile/change/username",
                (url, info, sessionID, _) => { return launcherCallbacks.ChangeUsername(url, info as ChangeRequestData, sessionID); },
                typeof(ChangeRequestData)
            ),
            new RouteAction(
                "/launcher/profile/change/password",
                (url, info, sessionID, _) => { return launcherCallbacks.ChangePassword(url, info as ChangeRequestData, sessionID); },
                typeof(ChangeRequestData)
            ),
            new RouteAction(
                "/launcher/profile/change/wipe",
                (url, info, sessionID, _) => { return launcherCallbacks.Wipe(url, info as RegisterData, sessionID); },
                typeof(RegisterData)
            ),
            new RouteAction(
                "/launcher/profile/remove",
                (url, info, sessionID, _) => { return launcherCallbacks.RemoveProfile(url, info as RemoveProfileData, sessionID); },
                typeof(RemoveProfileData)
            ),
            new RouteAction(
                "/launcher/profile/compatibleTarkovVersion",
                (_, _, _, _) => { return launcherCallbacks.GetCompatibleTarkovVersion(); }),
            new RouteAction(
                "/launcher/server/version",
                (_, _, _, _) => { return launcherCallbacks.GetServerVersion(); }),
            new RouteAction(
                "/launcher/server/loadedServerMods",
                (_, _, _, _) => { return launcherCallbacks.GetLoadedServerMods(); }),
            new RouteAction(
                "/launcher/server/serverModsUsedByProfile",
                (url, info, sessionID, _) => { return launcherCallbacks.GetServerModsProfileUsed(url, info as EmptyRequestData, sessionID); },
                typeof(EmptyRequestData)
            )
        ]
    )
    {
    }
}
