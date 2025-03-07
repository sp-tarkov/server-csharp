using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class LauncherCallbacks(
    HttpResponseUtil _httpResponseUtil,
    LauncherController _launcherController,
    SaveServer _saveServer,
    Watermark _watermark
)
{
    public string Connect()
    {
        return _httpResponseUtil.NoBody(_launcherController.Connect());
    }

    public string Login(string url, LoginRequestData info, string sessionID)
    {
        var output = _launcherController.Login(info);
        return output ?? "FAILED";
    }

    public string Register(string url, RegisterData info, string sessionID)
    {
        var output = _launcherController.Register(info);
        return string.IsNullOrEmpty(output) ? "FAILED" : "OK";
    }

    public string Get(string url, LoginRequestData info, string sessionID)
    {
        var output = _launcherController.Find(_launcherController.Login(info));
        return _httpResponseUtil.NoBody(output);
    }

    public string ChangeUsername(string url, ChangeRequestData info, string sessionID)
    {
        var output = _launcherController.ChangeUsername(info);
        return string.IsNullOrEmpty(output) ? "FAILED" : "OK";
    }

    public string ChangePassword(string url, ChangeRequestData info, string sessionID)
    {
        var output = _launcherController.ChangePassword(info);
        return string.IsNullOrEmpty(output) ? "FAILED" : "OK";
    }

    public string Wipe(string url, RegisterData info, string sessionID)
    {
        var output = _launcherController.Wipe(info);
        return string.IsNullOrEmpty(output) ? "FAILED" : "OK";
    }

    public string GetServerVersion()
    {
        return _httpResponseUtil.NoBody(_watermark.GetVersionTag());
    }

    public string Ping(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody("pong!");
    }

    public string RemoveProfile(string url, RemoveProfileData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_saveServer.RemoveProfile(sessionID));
    }

    public string GetCompatibleTarkovVersion()
    {
        return _httpResponseUtil.NoBody(_launcherController.GetCompatibleTarkovVersion());
    }

    public string GetLoadedServerMods()
    {
        return _httpResponseUtil.NoBody(_launcherController.GetLoadedServerMods());
    }

    public string GetServerModsProfileUsed(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody(_launcherController.GetServerModsProfileUsed(sessionID));
    }
}
