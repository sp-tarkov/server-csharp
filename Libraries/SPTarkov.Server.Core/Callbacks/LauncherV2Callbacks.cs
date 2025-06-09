using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Spt.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class LauncherV2Callbacks(
    HttpResponseUtil _httpResponseUtil,
    LauncherV2Controller _launcherV2Controller,
    ProfileController _profileController
)
{
    public ValueTask<string> Ping()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2PingResponse
            {
                Response = _launcherV2Controller.Ping()
            }
        ));
    }

    public ValueTask<string> Types()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2TypesResponse
            {
                Response = _launcherV2Controller.Types()
            }
        ));
    }

    public ValueTask<string> Login(LoginRequestData info)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2LoginResponse
            {
                Response = _launcherV2Controller.Login(info)
            }
        ));
    }

    public async ValueTask<string> Register(RegisterData info)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2RegisterResponse
            {
                Response = await _launcherV2Controller.Register(info),
                Profiles = _profileController.GetMiniProfiles()
            }
        );
    }

    public async ValueTask<string> PasswordChange(ChangeRequestData info)
    {
        return _httpResponseUtil.NoBody(
            new LauncherV2PasswordChangeResponse
            {
                Response = await _launcherV2Controller.PasswordChange(info),
                Profiles = _profileController.GetMiniProfiles()
            }
        );
    }

    public ValueTask<string> Remove(LoginRequestData info)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2RemoveResponse
            {
                Response = _launcherV2Controller.Remove(info),
                Profiles = _profileController.GetMiniProfiles()
            }
        ));
    }

    public ValueTask<string> CompatibleVersion()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2VersionResponse
            {
                Response = new LauncherV2CompatibleVersion
                {
                    SptVersion = _launcherV2Controller.SptVersion(),
                    EftVersion = _launcherV2Controller.EftVersion()
                }
            }
        ));
    }

    public ValueTask<string> Mods()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2ModsResponse
            {
                Response = _launcherV2Controller.LoadedMods()
            }
        ));
    }

    public ValueTask<string> Profiles()
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2ProfilesResponse
            {
                Response = _profileController.GetMiniProfiles()
            }
        ));
    }

    public ValueTask<string> Profile(string? sessionId)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new LauncherV2ProfileResponse
            {
                Response = _launcherV2Controller.GetProfile(sessionId)
            }
        ));
    }
}
