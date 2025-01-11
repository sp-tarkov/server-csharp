using Core.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Game;
using Core.Models.Eft.HttpResponse;
using Core.Servers;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class GameCallbacks : OnLoad
{
    protected HttpResponseUtil _httpResponseUtil;
    protected Watermark _watermark;
    protected SaveServer _saveServer;
    protected GameController _gameController;
    protected TimeUtil _timeUtil;

    public GameCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        Watermark watermark,
        SaveServer saveServer,
        GameController gameController,
        TimeUtil timeUtil
    )
    {
        _httpResponseUtil = httpResponseUtil;
        _watermark = watermark;
        _saveServer = saveServer;
        _gameController = gameController;
        _timeUtil = timeUtil;
    }

    public async Task OnLoad()
    {
        _gameController.Load();
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
        var today = _timeUtil.GetDate();
        var startTimestampMS = _timeUtil.GetTimeStamp();
        _gameController.GameStart(url, info, sessionID, startTimestampMS);
        return _httpResponseUtil.GetBody(new GameStartResponse() { UtcTime = startTimestampMS / 1000 });
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
    public string GetSurveyView(string url, object info, string sessionID)
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
