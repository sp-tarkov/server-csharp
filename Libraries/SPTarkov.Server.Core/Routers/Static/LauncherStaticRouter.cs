using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class LauncherStaticRouter : StaticRouter
{
    public LauncherStaticRouter(LauncherCallbacks launcherCallbacks, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(
                "/launcher/ping",
                async (url, _, sessionID, _) => await launcherCallbacks.Ping(url, null, sessionID)
            ),
            new RouteAction(
                "/launcher/server/connect",
                async (_, _, _, _) => await launcherCallbacks.Connect()
            ),
            new RouteAction(
                "/launcher/profile/login",
                async (url, info, sessionID, _) => await launcherCallbacks.Login(url, info as LoginRequestData, sessionID),
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/profile/register",
                async (url, info, sessionID, _) => await launcherCallbacks.Register(url, info as RegisterData, sessionID),
                typeof(RegisterData)
            ),
            new RouteAction(
                "/launcher/profile/get",
                async (url, info, sessionID, _) => await launcherCallbacks.Get(url, info as LoginRequestData, sessionID),
                typeof(LoginRequestData)
            ),
            new RouteAction(
                "/launcher/profile/change/username",
                async (url, info, sessionID, _) =>
                    await launcherCallbacks.ChangeUsername(url, info as ChangeRequestData, sessionID),
                typeof(ChangeRequestData)
            ),
            new RouteAction(
                "/launcher/profile/change/password",
                async (url, info, sessionID, _) =>
                    await launcherCallbacks.ChangePassword(url, info as ChangeRequestData, sessionID),
                typeof(ChangeRequestData)
            ),
            new RouteAction(
                "/launcher/profile/change/wipe",
                async (url, info, sessionID, _) => await launcherCallbacks.Wipe(url, info as RegisterData, sessionID),
                typeof(RegisterData)
            ),
            new RouteAction(
                "/launcher/profile/remove",
                async (url, info, sessionID, _) => await launcherCallbacks.RemoveProfile(url, info as RemoveProfileData, sessionID),
                typeof(RemoveProfileData)
            ),
            new RouteAction(
                "/launcher/profile/compatibleTarkovVersion",
                async (_, _, _, _) => await launcherCallbacks.GetCompatibleTarkovVersion()
            ),
            new RouteAction(
                "/launcher/server/version",
                async (_, _, _, _) => await launcherCallbacks.GetServerVersion()
            ),
            new RouteAction(
                "/launcher/server/loadedServerMods",
                async (_, _, _, _) => await launcherCallbacks.GetLoadedServerMods()
            ),
            new RouteAction(
                "/launcher/server/serverModsUsedByProfile",
                async (url, info, sessionID, _) =>
                    await launcherCallbacks.GetServerModsProfileUsed(url, info as EmptyRequestData, sessionID),
                typeof(EmptyRequestData)
            )
        ]
    )
    {
    }
}
