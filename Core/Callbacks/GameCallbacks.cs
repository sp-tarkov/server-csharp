namespace Core.Callbacks;

public class GameCallbacks
{
    public NullResponseData VersionValidata(string url, VersionValidaterequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GameStart(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GameLogout(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GameConfigResponse> GetGameConfig(string url, GameEmptyCrcRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GetServer(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> ValidateGameVersion(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GameKeepalive(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetVersion(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}