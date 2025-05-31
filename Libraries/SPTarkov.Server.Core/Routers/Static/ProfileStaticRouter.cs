using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class ProfileStaticRouter : StaticRouter
{
    public ProfileStaticRouter(ProfileCallbacks profileCallbacks, JsonUtil jsonUtil) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/game/profile/create",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.CreateProfile(url, info as ProfileCreateRequestData, sessionID),
                typeof(ProfileCreateRequestData)
            ),
            new RouteAction(
                "/client/game/profile/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.GetProfileData(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/profile/savage/regenerate",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.RegenerateScav(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/profile/voice/change",
                async (url, info, sessionID, output) =>
                    await profileCallbacks.ChangeVoice(url, info as ProfileChangeVoiceRequestData, sessionID),
                typeof(ProfileChangeVoiceRequestData)
            ),
            new RouteAction(
                "/client/game/profile/nickname/change",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.ChangeNickname(url, info as ProfileChangeNicknameRequestData, sessionID),
                typeof(ProfileChangeNicknameRequestData)
            ),
            new RouteAction(
                "/client/game/profile/nickname/validate",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.ValidateNickname(url, info as ValidateNicknameRequestData, sessionID),
                typeof(ValidateNicknameRequestData)
            ),
            new RouteAction(
                "/client/game/profile/nickname/reserved",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.GetReservedNickname(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/profile/status",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.GetProfileStatus(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/profile/view",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.GetOtherProfile(url, info as GetOtherProfileRequest, sessionID),
                typeof(GetOtherProfileRequest)
            ),
            new RouteAction(
                "/client/profile/settings",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.GetProfileSettings(url, info as GetProfileSettingsRequest, sessionID),
                typeof(GetProfileSettingsRequest)
            ),
            new RouteAction(
                "/client/game/profile/search",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await profileCallbacks.SearchProfiles(url, info as SearchProfilesRequestData, sessionID),
                typeof(SearchProfilesRequestData)
            ),
            new RouteAction(
                "/launcher/profile/info",
                async (url, info, sessionID, output) =>
                    await profileCallbacks.GetMiniProfile(url, info as GetMiniProfileRequestData, sessionID),
                typeof(GetMiniProfileRequestData)
            ),
            new RouteAction(
                "/launcher/profiles",
                async (url, info, sessionID, output) =>
                    await profileCallbacks.GetAllMiniProfiles(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
