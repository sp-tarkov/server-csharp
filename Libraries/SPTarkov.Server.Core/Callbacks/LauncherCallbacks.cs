using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class LauncherCallbacks(
    HttpResponseUtil httpResponseUtil,
    LauncherController launcherController,
    SaveServer saveServer,
    Watermark watermark
)
{
    public ValueTask<string> Connect()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(launcherController.Connect()));
    }

    public ValueTask<string> Login(string url, LoginRequestData info, MongoId sessionID)
    {
        var output = launcherController.Login(info);
        return new ValueTask<string>(output.IsEmpty ? "FAILED" : output.ToString());
    }

    public async ValueTask<string> Register(string url, RegisterData info, MongoId sessionID)
    {
        var output = await launcherController.Register(info);
        return output.IsEmpty ? string.Empty : output.ToString();
    }

    public ValueTask<string> Get(string url, LoginRequestData info, MongoId sessionID)
    {
        var output = launcherController.Find(launcherController.Login(info));
        return new ValueTask<string>(httpResponseUtil.NoBody(output));
    }

    public ValueTask<string> ChangeUsername(string url, ChangeRequestData info, MongoId sessionID)
    {
        var output = launcherController.ChangeUsername(info);
        return new ValueTask<string>(string.IsNullOrEmpty(output) ? "FAILED" : "OK");
    }

    public ValueTask<string> Wipe(string url, RegisterData info, MongoId sessionID)
    {
        var output = launcherController.Wipe(info);
        return new ValueTask<string>(output.IsEmpty ? "FAILED" : "OK");
    }

    public ValueTask<string> GetServerVersion()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(watermark.GetVersionTag()));
    }

    public ValueTask<string> Ping(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody("pong!"));
    }

    public ValueTask<string> RemoveProfile(string url, RemoveProfileData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(saveServer.RemoveProfile(sessionID)));
    }

    public ValueTask<string> GetCompatibleTarkovVersion()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(launcherController.GetCompatibleTarkovVersion()));
    }

    public ValueTask<string> GetLoadedServerMods()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(launcherController.GetLoadedServerMods()));
    }

    public ValueTask<string> GetServerModsProfileUsed(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(launcherController.GetServerModsProfileUsed(sessionID)));
    }
}
