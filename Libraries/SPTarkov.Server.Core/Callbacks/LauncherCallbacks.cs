using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class LauncherCallbacks(
    HttpResponseUtil _httpResponseUtil,
    LauncherController _launcherController,
    SaveServer _saveServer,
    Watermark _watermark
)
{
    public ValueTask<string> Connect()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_launcherController.Connect()));
    }

    public ValueTask<string> Login(string url, LoginRequestData info, string sessionID)
    {
        var output = _launcherController.Login(info);
        return new ValueTask<string>(output ?? "FAILED");
    }

    public async ValueTask<string> Register(string url, RegisterData info, string sessionID)
    {
        var output = await _launcherController.Register(info);
        return string.IsNullOrEmpty(output) ? "FAILED" : "OK";
    }

    public ValueTask<string> Get(string url, LoginRequestData info, string sessionID)
    {
        var output = _launcherController.Find(_launcherController.Login(info));
        return new ValueTask<string>(_httpResponseUtil.NoBody(output));
    }

    public ValueTask<string> ChangeUsername(string url, ChangeRequestData info, string sessionID)
    {
        var output = _launcherController.ChangeUsername(info);
        return new ValueTask<string>(string.IsNullOrEmpty(output) ? "FAILED" : "OK");
    }

    public ValueTask<string> ChangePassword(string url, ChangeRequestData info, string sessionID)
    {
        var output = _launcherController.ChangePassword(info);
        return new ValueTask<string>(string.IsNullOrEmpty(output) ? "FAILED" : "OK");
    }

    public ValueTask<string> Wipe(string url, RegisterData info, string sessionID)
    {
        var output = _launcherController.Wipe(info);
        return new ValueTask<string>(string.IsNullOrEmpty(output) ? "FAILED" : "OK");
    }

    public ValueTask<string> GetServerVersion()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_watermark.GetVersionTag()));
    }

    public ValueTask<string> Ping(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody("pong!"));
    }

    public ValueTask<string> RemoveProfile(string url, RemoveProfileData info, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_saveServer.RemoveProfile(sessionID)));
    }

    public ValueTask<string> GetCompatibleTarkovVersion()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_launcherController.GetCompatibleTarkovVersion()));
    }

    public ValueTask<string> GetLoadedServerMods()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_launcherController.GetLoadedServerMods()));
    }

    public ValueTask<string> GetServerModsProfileUsed(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_launcherController.GetServerModsProfileUsed(sessionID)));
    }
}
