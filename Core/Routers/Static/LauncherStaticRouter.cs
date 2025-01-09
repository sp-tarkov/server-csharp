using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class LauncherStaticRouter : StaticRouter {
    
    public LauncherStaticRouter(LauncherCallbacks launcherCallbacks) : base([
        new RouteAction<EmptyRequestData?>(
            "/launcher/ping",
            (url, _, sessionID, _) => launcherCallbacks.Ping(url, null, sessionID)),
        new RouteAction<EmptyRequestData?>(
            "/launcher/server/connect",
            (_, _, _, _) => launcherCallbacks.Connect()),
        new RouteAction(
            "/launcher/profile/login",
            (url, info, sessionID, _) => launcherCallbacks.Login(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/register",
            (url, info, sessionID, _) => launcherCallbacks.Register(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/get",
            (url, info, sessionID, _) => launcherCallbacks.Get(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/change/username",
            (url, info, sessionID, _) => launcherCallbacks.ChangeUsername(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/change/password",
            (url, info, sessionID, _) => launcherCallbacks.ChangePassword(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/change/wipe",
            (url, info, sessionID, _) => launcherCallbacks.Wipe(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/remove",
            (url, info, sessionID, _) => launcherCallbacks.RemoveProfile(url, info, sessionID)),
        new RouteAction(
            "/launcher/profile/compatibleTarkovVersion",
            (_, _, _, _) => launcherCallbacks.GetCompatibleTarkovVersion()),
        new RouteAction(
            "/launcher/server/version",
            (_, _, _, _) => launcherCallbacks.GetServerVersion()),
        new RouteAction(
            "/launcher/server/loadedServerMods",
            (_, _, _, _) => launcherCallbacks.GetLoadedServerMods()),
        new RouteAction(
            "/launcher/server/serverModsUsedByProfile",
            (url, info, sessionID, _) => launcherCallbacks.GetServerModsProfileUsed(url, info, sessionID)),
    ])
    {
    }

    public override Type? GetBodyDeserializationType()
    {
        throw new NotImplementedException();
    }
}
}
