using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

public class ProfileCallbacks
{
    public ProfileCallbacks()
    {
        
    }
    
    /// <summary>
    /// Handle client/game/profile/create
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<CreateProfileResponse> CreateProfile(string url, ProfileCreateRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/game/profile/list
    /// Get the complete player profile (scav + pmc character)
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<object> GetProfileData(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/game/profile/savage/regenerate
    /// Handle the creation of a scav profile for player
    /// Occurs post-raid and when profile first created immediately after character details are confirmed by player
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<object> RegenerateScav(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/game/profile/voice/change event
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData ChangeVoice(string url, ProfileChangeVoiceRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/game/profile/nickname/change event
    /// Client allows player to adjust their profile name
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<object> ChangeNickname(string url, ProfileChangeNicknameRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/game/profile/nickname/validate
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<object> ValidateNickname(string url, ValidateNicknameRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/game/profile/nickname/reserved
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<string> GetReservedNickname(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/profile/status
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetProfileStatusResponseData> GetProfileStatus(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/profile/status
    /// Called when creating a character when choosing a character face/voice
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetOtherProfileResponse> GetOtherProfile(string url, GetOtherProfileRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/profile/view
    /// Called when viewing another players profile
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<bool> GetProfileSettings(string url, GetProfileSettingsRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/profile/settings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<SearchFriendResponse> SearchFriend(string url, SearchFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle launcher/profile/info
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetMiniProfile(string url, GetMiniProfileRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /launcher/profiles
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetAllMiniProfiles(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}