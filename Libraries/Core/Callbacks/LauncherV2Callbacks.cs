using Core.Controllers;
using Core.Models.Eft.Launcher;
using Core.Models.Spt.Launcher;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class LauncherV2Callbacks(
    HttpResponseUtil _httpResponseUtil,
    LauncherV2Controller _launcherV2Controller,
    ProfileController _profileController
)
{
    public string Ping()
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2PingResponse
            {
                Response = _launcherV2Controller.Ping()
            }
        );
    }

    public string Types()
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2TypesResponse
            {
                Response = _launcherV2Controller.Types()
            }
        );
    }

    public string Login(LoginRequestData info)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2LoginResponse
            {
                Response = _launcherV2Controller.Login(info)
            }
        );
    }

    public string Register(RegisterData info)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2RegisterResponse
            {
                Response = _launcherV2Controller.Register(info),
                Profiles = _profileController.GetMiniProfiles()
            }
        );
    }

    public string PasswordChange(ChangeRequestData info)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2PasswordChangeResponse
            {
                Response = _launcherV2Controller.PasswordChange(info),
                Profiles = _profileController.GetMiniProfiles()
            }
        );
    }

    public string Remove(LoginRequestData info)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2RemoveResponse
            {
                Response = _launcherV2Controller.Remove(info),
                Profiles = _profileController.GetMiniProfiles()
            }
        );
    }

    public string CompatibleVersion()
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2VersionResponse
            {
                Response = new LauncherV2CompatibleVersion
                {
                    SptVersion = _launcherV2Controller.SptVersion(),
                    EftVersion = _launcherV2Controller.EftVersion()
                }
            }
        );
    }

    public string Mods()
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2ModsResponse
            {
                Response = _launcherV2Controller.LoadedMods()
            }
        );
    }

    public string Profiles()
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2ProfilesResponse
            {
                Response = _profileController.GetMiniProfiles()
            }
        );
    }

    public object Profile(string? sessionId)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2ProfileResponse
            {
                Response = _launcherV2Controller.GetProfile(sessionId)
            }
        );
    }
}
