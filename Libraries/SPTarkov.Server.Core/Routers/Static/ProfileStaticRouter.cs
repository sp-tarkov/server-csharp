using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class ProfileStaticRouter(ProfileCallbacks profileCallbacks, JsonUtil jsonUtil)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ProfileCreateRequestData>(
                "/client/game/profile/create",
                async (url, info, sessionID, output) => await profileCallbacks.CreateProfile(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/game/profile/list",
                async (url, info, sessionID, output) => await profileCallbacks.GetProfileData(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/game/profile/savage/regenerate",
                async (url, info, sessionID, output) => await profileCallbacks.RegenerateScav(url, info, sessionID)
            ),
            new RouteAction<ProfileChangeVoiceRequestData>(
                "/client/game/profile/voice/change",
                async (url, info, sessionID, output) => await profileCallbacks.ChangeVoice(url, info, sessionID)
            ),
            new RouteAction<ProfileChangeNicknameRequestData>(
                "/client/game/profile/nickname/change",
                async (url, info, sessionID, output) => await profileCallbacks.ChangeNickname(url, info, sessionID)
            ),
            new RouteAction<ValidateNicknameRequestData>(
                "/client/game/profile/nickname/validate",
                async (url, info, sessionID, output) => await profileCallbacks.ValidateNickname(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/game/profile/nickname/reserved",
                async (url, info, sessionID, output) => await profileCallbacks.GetReservedNickname(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/profile/status",
                async (url, info, sessionID, output) => await profileCallbacks.GetProfileStatus(url, info, sessionID)
            ),
            new RouteAction<GetOtherProfileRequest>(
                "/client/profile/view",
                async (url, info, sessionID, output) => await profileCallbacks.GetOtherProfile(url, info, sessionID)
            ),
            new RouteAction<GetProfileSettingsRequest>(
                "/client/profile/settings",
                async (url, info, sessionID, output) => await profileCallbacks.GetProfileSettings(url, info, sessionID)
            ),
            new RouteAction<SearchProfilesRequestData>(
                "/client/game/profile/search",
                async (url, info, sessionID, output) => await profileCallbacks.SearchProfiles(url, info, sessionID)
            ),
            new RouteAction<GetMiniProfileRequestData>(
                "/launcher/profile/info",
                async (url, info, sessionID, output) => await profileCallbacks.GetMiniProfile(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/launcher/profiles",
                async (url, info, sessionID, output) => await profileCallbacks.GetAllMiniProfiles(url, info, sessionID)
            ),
        ]
    ) { }
