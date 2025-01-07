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
    
    public GetBodyResponseData<CreateProfileResponse> CreateProfile(string url, ProfileCreateRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<object> GetProfileData(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<object> RegenerateScav(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public NullResponseData ChangeVoice(string url, ProfileChangeVoiceRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<object> ChangeNickname(string url, ProfileChangeNicknameRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<object> ValidateNickname(string url, ValidateNicknameRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<string> GetReservedNickname(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetOtherProfileResponse> GetOtherProfile(string url, GetOtherProfileRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> GetProfileSettings(string url, GetProfileSettingsRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<SearchFriendResponse> SearchFriend(string url, SearchFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetMiniProfile(string url, GetMiniProfileRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetAllMiniProfiles(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}