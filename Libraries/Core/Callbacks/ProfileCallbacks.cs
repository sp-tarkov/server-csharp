using Core.Controllers;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class ProfileCallbacks(
    HttpResponseUtil _httpResponse,
    TimeUtil _timeUtil,
    ProfileController _profileController,
    ProfileHelper _profileHelper
)
{
    /**
     * Handle client/game/profile/create
     */
    public string CreateProfile(string url, ProfileCreateRequestData info, string sessionID)
    {
        var id = _profileController.CreateProfile(info, sessionID);
        return _httpResponse.GetBody(
            new CreateProfileResponse
            {
                UserId = id
            }
        );
    }

    /**
     * Handle client/game/profile/list
     * Get the complete player profile (scav + pmc character)
     */
    public string GetProfileData(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.GetCompleteProfile(sessionID));
    }

    /**
     * Handle client/game/profile/savage/regenerate
     * Handle the creation of a scav profile for player
     * Occurs post-raid and when profile first created immediately after character details are confirmed by player
     * @param url
     * @param info empty
     * @param sessionID Session id
     * @returns Profile object
     */
    public string RegenerateScav(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.GetBody(
            new List<PmcData>
            {
                _profileController.GeneratePlayerScav(sessionID)
            }
        );
    }

    /**
     * Handle client/game/profile/voice/change event
     */
    public string ChangeVoice(string url, ProfileChangeVoiceRequestData info, string sessionID)
    {
        _profileController.ChangeVoice(info, sessionID);
        return _httpResponse.NullResponse();
    }

    /**
     * Handle client/game/profile/nickname/change event
     * Client allows player to adjust their profile name
     */
    public string ChangeNickname(string url, ProfileChangeNicknameRequestData info, string sessionID)
    {
        var output = _profileController.ChangeNickname(info, sessionID);

        return output switch
        {
            "taken" => _httpResponse.GetBody<object?>(null, BackendErrorCodes.NicknameNotUnique, "The nickname is already in use"),
            "tooshort" => _httpResponse.GetBody<object?>(null, BackendErrorCodes.NicknameNotValid, "The nickname is too short"),
            _ => _httpResponse.GetBody<object>(
                new
                {
                    status = 0,
                    nicknamechangedate = _timeUtil.GetTimeStamp()
                }
            )
        };
    }

    /**
     * Handle client/game/profile/nickname/validate
     */
    public string ValidateNickname(string url, ValidateNicknameRequestData info, string sessionID)
    {
        var output = _profileController.ValidateNickname(info, sessionID);

        return output switch
        {
            "taken" => _httpResponse.GetBody<object?>(null, BackendErrorCodes.NicknameNotUnique, "The nickname is already in use"),
            "tooshort" => _httpResponse.GetBody<object?>(null, BackendErrorCodes.NicknameNotValid, "The nickname is too short"),
            _ => _httpResponse.GetBody(
                new
                {
                    status = "ok"
                }
            )
        };
    }

    /**
     * Handle client/game/profile/nickname/reserved
     */
    public string GetReservedNickname(string url, EmptyRequestData _, string sessionID)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionID);
        if (fullProfile?.ProfileInfo?.Username is not null)
        {
            return _httpResponse.GetBody(fullProfile?.ProfileInfo?.Username);
        }

        return _httpResponse.GetBody("SPTarkov");
    }

    /**
     * Handle client/profile/status
     * Called when creating a character when choosing a character face/voice
     */
    public string GetProfileStatus(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.GetProfileStatus(sessionID));
    }

    /**
     * Handle client/profile/view
     * Called when viewing another players profile
     */
    public string GetOtherProfile(string url, GetOtherProfileRequest request, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.GetOtherProfile(sessionID, request));
    }

    /**
     * Handle client/profile/settings
     */
    public string GetProfileSettings(string url, GetProfileSettingsRequest info, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.SetChosenProfileIcon(sessionID, info));
    }

    /**
     * Handle client/game/profile/search
     */
    public string SearchProfiles(string url, SearchProfilesRequestData info, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.SearchProfiles(info, sessionID));
    }

    /**
     * Handle launcher/profile/info
     */
    public string GetMiniProfile(string url, GetMiniProfileRequestData info, string sessionID)
    {
        return _httpResponse.NoBody(_profileController.GetMiniProfile(sessionID));
    }

    /**
     * Handle /launcher/profiles
     */
    public string GetAllMiniProfiles(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.NoBody(_profileController.GetMiniProfiles());
    }
}
