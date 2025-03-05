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
    /// <summary>
    /// Handle client/game/profile/create
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Handle client/game/profile/list
    /// Get the complete player profile (scav + pmc character) 
    /// </summary>
    /// <returns></returns>
    public string GetProfileData(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.GetCompleteProfile(sessionID));
    }

    /// <summary>
    /// Handle client/game/profile/savage/regenerate
    /// Handle the creation of a scav profile for player
    /// Occurs post-raid and when profile first created immediately after character details are confirmed by player
    /// </summary>
    /// <returns></returns>
    public string RegenerateScav(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.GetBody(
            new List<PmcData>
            {
                _profileController.GeneratePlayerScav(sessionID)
            }
        );
    }

    /// <summary>
    /// Handle client/game/profile/voice/change event
    /// </summary>
    /// <returns></returns>
    public string ChangeVoice(string url, ProfileChangeVoiceRequestData info, string sessionID)
    {
        _profileController.ChangeVoice(info, sessionID);
        return _httpResponse.NullResponse();
    }

    /// <summary>
    /// Handle client/game/profile/nickname/change event
    /// Client allows player to adjust their profile name
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Handle client/game/profile/nickname/validate
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Handle client/game/profile/nickname/reserved
    /// </summary>
    /// <returns></returns>
    public string GetReservedNickname(string url, EmptyRequestData _, string sessionID)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionID);
        if (fullProfile?.ProfileInfo?.Username is not null)
        {
            return _httpResponse.GetBody(fullProfile?.ProfileInfo?.Username);
        }

        return _httpResponse.GetBody("SPTarkov");
    }

    /// <summary>
    /// Handle client/profile/status
    /// Called when creating a character when choosing a character face/voice
    /// </summary>
    /// <returns></returns>
    public string GetProfileStatus(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.GetProfileStatus(sessionID));
    }

    /// <summary>
    /// Handle client/profile/view
    /// Called when viewing another players profile
    /// </summary>
    /// <returns></returns>
    public string GetOtherProfile(string url, GetOtherProfileRequest request, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.GetOtherProfile(sessionID, request));
    }

    /// <summary>
    /// Handle client/profile/settings
    /// </summary>
    /// <returns></returns>
    public string GetProfileSettings(string url, GetProfileSettingsRequest info, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.SetChosenProfileIcon(sessionID, info));
    }

    /// <summary>
    /// Handle client/game/profile/search
    /// </summary>
    /// <returns></returns>
    public string SearchProfiles(string url, SearchProfilesRequestData info, string sessionID)
    {
        return _httpResponse.GetBody(_profileController.SearchProfiles(info, sessionID));
    }

    /// <summary>
    /// Handle launcher/profile/info
    /// </summary>
    /// <returns></returns>
    public string GetMiniProfile(string url, GetMiniProfileRequestData info, string sessionID)
    {
        return _httpResponse.NoBody(_profileController.GetMiniProfile(sessionID));
    }

    /// <summary>
    /// Handle /launcher/profiles
    /// </summary>
    /// <returns></returns>
    public string GetAllMiniProfiles(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponse.NoBody(_profileController.GetMiniProfiles());
    }
}
