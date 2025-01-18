using Core.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Game;
using Core.Servers;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.GameCallbacks)]
[Injectable(InjectableTypeOverride = typeof(GameCallbacks))]
public class GameCallbacks(
    HttpResponseUtil _httpResponseUtil,
    Watermark _watermark,
    SaveServer _saveServer,
    GameController _gameController,
    TimeUtil _timeUtil
) : OnLoad
{
    public Task OnLoad()
    {
        _gameController.Load();
        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-game";
    }

    /// <summary>
    /// Handle client/game/version/validate
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string VersionValidate(string url, VersionValidateRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/game/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GameStart(string url, EmptyRequestData info, string sessionID)
    {
        var startTimestampSec = _timeUtil.GetTimeStamp();
        _gameController.GameStart(url, info, sessionID, startTimestampSec);
        return _httpResponseUtil.GetBody(new GameStartResponse() { UtcTime = startTimestampSec });
    }

    /// <summary>
    /// Handle client/game/logout
    /// Save profiles on game close
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GameLogout(string url, EmptyRequestData info, string sessionID)
    {
        _saveServer.Save();
        return _httpResponseUtil.GetBody(new GameLogoutResponseData() { Status = "ok" });
    }

    /// <summary>
    /// Handle client/game/config
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetGameConfig(string url, GameEmptyCrcRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetGameConfig(sessionID));
    }

    /// <summary>
    /// Handle client/game/mode
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetGameMode(string url, GameModeRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetGameMode(sessionID, info));
    }

    /// <summary>
    /// Handle client/server/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetServer(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetServer(sessionID));
    }

    /// <summary>
    /// Handle client/match/group/current
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetCurrentGroup(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetCurrentGroup(sessionID));
    }

    /// <summary>
    /// Handle client/checkVersion
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ValidateGameVersion(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetValidGameVersion(sessionID));
    }

    /// <summary>
    /// Handle client/game/keepalive
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GameKeepalive(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetKeepAlive(sessionID));
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
        // change to be a proper type
        return _httpResponseUtil.NoBody(new { Version = _watermark.GetInGameVersionLabel() });
    }

    /// <summary>
    /// Handle /client/report/send & /client/reports/lobby/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ReportNickname(string url, UIDRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetRaidTime(string url, GetRaidTimeRequest info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_gameController.GetRaidTime(sessionID, info));
    }

    /// <summary>
    /// Handle /client/survey
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetSurvey(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetSurvey(sessionID));
    }

    /// <summary>
    /// Handle client/survey/view
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetSurveyView(string url, SendSurveyOpinionRequest info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/survey/opinion
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SendSurveyOpinion(string url, SendSurveyOpinionRequest info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }
}
