using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Spt.Launcher;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class LauncherV2Callbacks(
    HttpResponseUtil httpResponseUtil,
    LauncherV2Controller launcherV2Controller,
    ProfileController profileController
)
{
    public ValueTask<string> Ping()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(new LauncherV2PingResponse { Response = launcherV2Controller.Ping() }));
    }

    public ValueTask<string> Types()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(new LauncherV2TypesResponse { Response = launcherV2Controller.Types() }));
    }

    public ValueTask<string> Login(LoginRequestData info)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(new LauncherV2LoginResponse { Response = launcherV2Controller.Login(info) }));
    }

    public async ValueTask<string> Register(RegisterData info)
    {
        return httpResponseUtil.NoBody(
            new LauncherV2RegisterResponse
            {
                Response = await launcherV2Controller.Register(info),
                Profiles = profileController.GetMiniProfiles(),
            }
        );
    }

    public async ValueTask<string> PasswordChange(ChangeRequestData info)
    {
        return httpResponseUtil.NoBody(
            new LauncherV2PasswordChangeResponse
            {
                Response = await launcherV2Controller.PasswordChange(info),
                Profiles = profileController.GetMiniProfiles(),
            }
        );
    }

    public ValueTask<string> Remove(LoginRequestData info)
    {
        return new ValueTask<string>(
            httpResponseUtil.NoBody(
                new LauncherV2RemoveResponse
                {
                    Response = launcherV2Controller.Remove(info),
                    Profiles = profileController.GetMiniProfiles(),
                }
            )
        );
    }

    public ValueTask<string> CompatibleVersion()
    {
        return new ValueTask<string>(
            httpResponseUtil.NoBody(
                new LauncherV2VersionResponse
                {
                    Response = new LauncherV2CompatibleVersion
                    {
                        SptVersion = launcherV2Controller.SptVersion(),
                        EftVersion = launcherV2Controller.EftVersion(),
                    },
                }
            )
        );
    }

    public ValueTask<string> Mods()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(new LauncherV2ModsResponse { Response = launcherV2Controller.LoadedMods() }));
    }

    public ValueTask<string> Profiles()
    {
        return new ValueTask<string>(
            httpResponseUtil.NoBody(new LauncherV2ProfilesResponse { Response = profileController.GetMiniProfiles() })
        );
    }

    public ValueTask<string> Profile(MongoId sessionId)
    {
        return new ValueTask<string>(
            httpResponseUtil.NoBody(new LauncherV2ProfileResponse { Response = launcherV2Controller.GetProfile(sessionId) })
        );
    }
}
