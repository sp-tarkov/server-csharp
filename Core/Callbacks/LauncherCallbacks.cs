using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Launcher;
using Core.Servers;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class LauncherCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    protected LauncherController _launcherController;
    protected SaveServer _saveServer;
    protected Watermark _watermark;
    public LauncherCallbacks(
        HttpResponseUtil httpResponse,
        LauncherController launcherController,
        SaveServer saveServer,
        Watermark watermark)
    {
        _httpResponseUtil = httpResponse;
        _launcherController = launcherController;
        _saveServer = saveServer;
        _watermark = watermark;
    }

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

    public string Ping(string url, EmptyRequestData info, string sessionID)
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

    public string GetServerModsProfileUsed(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_launcherController.GetServerModsProfileUsed(sessionID));
    }
}
