using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Game;
using Core.Models.Eft.HttpResponse;

namespace Core.Callbacks;

public class GameCallbacks
{
    public GameCallbacks()
    {
        
    }

    public async Task OnLoad()
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
    
    public NullResponseData VersionValidata(string url, VersionValidateRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GameStartResponse> GameStart(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GameLogoutResponseData> GameLogout(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GameConfigResponse> GetGameConfig(string url, GameEmptyCrcRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GameModeResponse> GetGameMode(string url, GameModeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<ServerDetails>> GetServer(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<CheckVersionResponse> ValidateGameVersion(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GameKeepAliveResponse> GameKeepalive(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetVersion(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData ReportNickname(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetRaidTimeResponse GetRaidTime(string url, GetRaidTimeRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public object GetSurvey(string url, EmptyRequestData info, string sessionID) // TODO: Types given was NullResponseData | GetBodyResponseData<SurveyResponseData>
    {
        throw new NotImplementedException();
    }

    public NullResponseData GetSurveyView(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData SendSurveyOpinion(string url, SendSurveyOpinionRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}