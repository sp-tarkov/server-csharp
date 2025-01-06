namespace Core.Callbacks;

public class ProfileCallbacks
{
    public object OnLoad(string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> CreateProfile(string url, ProfileCreateRequestData info, string sessionID)
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
    
    public GetBodyResponseData<object> GetProfileStatus(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<SearchFriendResponse> SearchFriend(string url, SearchFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}