using Core.Annotations;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Game;
using Core.Models.Eft.HttpResponse;

namespace Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.GameCallbacks)]
public class GameCallbacks : OnLoad
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

    /// <summary>
    /// Handle client/game/version/validate
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData VersionValidate(string url, VersionValidateRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GameStartResponse> GameStart(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/logout
    /// Save profiles on game close
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GameLogoutResponseData> GameLogout(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/config
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GameConfigResponse> GetGameConfig(string url, GameEmptyCrcRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/mode
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GameModeResponse> GetGameMode(string url, GameModeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/server/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<ServerDetails>> GetServer(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/group/current
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<CurrentGroupResponse> GetCurrentGroup(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/checkVersion
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<CheckVersionResponse> ValidateGameVersion(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/keepalive
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GameKeepAliveResponse> GameKeepalive(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/version
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetVersion(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /client/report/send & /client/reports/lobby/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData ReportNickname(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetRaidTimeResponse GetRaidTime(string url, GetRaidTimeRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /client/survey
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public object GetSurvey(string url, EmptyRequestData info, string sessionID) // TODO: Types given was NullResponseData | GetBodyResponseData<SurveyResponseData>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/survey/view
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData GetSurveyView(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/survey/opinion
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData SendSurveyOpinion(string url, SendSurveyOpinionRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
