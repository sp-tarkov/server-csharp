using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.GameCallbacks)]
[Injectable(InjectableTypeOverride = typeof(GameCallbacks))]
public class GameCallbacks(
    HttpResponseUtil _httpResponseUtil,
    Watermark _watermark,
    SaveServer _saveServer,
    GameController _gameController,
    TimeUtil _timeUtil
) : IOnLoad
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
    ///     Handle client/game/version/validate
    /// </summary>
    /// <returns></returns>
    public string VersionValidate(string url, VersionValidateRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/game/start
    /// </summary>
    /// <returns></returns>
    public string GameStart(string url, EmptyRequestData _, string sessionID)
    {
        var startTimestampSec = _timeUtil.GetTimeStamp();
        _gameController.GameStart(url, sessionID, startTimestampSec);
        return _httpResponseUtil.GetBody(new GameStartResponse { UtcTime = startTimestampSec });
    }

    /// <summary>
    ///     Handle client/game/logout
    ///     Save profiles on game close
    /// </summary>
    /// <returns></returns>
    public string GameLogout(string url, EmptyRequestData _, string sessionID)
    {
        _saveServer.SaveProfile(sessionID);
        return _httpResponseUtil.GetBody(new GameLogoutResponseData { Status = "ok" });
    }

    /// <summary>
    ///     Handle client/game/config
    /// </summary>
    /// <returns></returns>
    public string GetGameConfig(string url, GameEmptyCrcRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetGameConfig(sessionID));
    }

    /// <summary>
    ///     Handle client/game/mode
    /// </summary>
    /// <returns></returns>
    public string GetGameMode(string url, GameModeRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetGameMode(sessionID, info));
    }

    /// <summary>
    ///     Handle client/server/list
    /// </summary>
    /// <returns></returns>
    public string GetServer(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetServer(sessionID));
    }

    /// <summary>
    ///     Handle client/match/group/current
    /// </summary>
    /// <returns></returns>
    public string GetCurrentGroup(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetCurrentGroup(sessionID));
    }

    /// <summary>
    ///     Handle client/checkVersion
    /// </summary>
    /// <returns></returns>
    public string ValidateGameVersion(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetValidGameVersion(sessionID));
    }

    /// <summary>
    ///     Handle client/game/keepalive
    /// </summary>
    /// <returns></returns>
    public string GameKeepalive(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetKeepAlive(sessionID));
    }

    /// <summary>
    ///     Handle singleplayer/settings/version
    /// </summary>
    /// <returns></returns>
    public string GetVersion(string url, EmptyRequestData _, string sessionID)
    {
        // change to be a proper type
        return _httpResponseUtil.NoBody(new { Version = _watermark.GetInGameVersionLabel() });
    }

    /// <summary>
    ///     Handle /client/report/send & /client/reports/lobby/send
    /// </summary>
    /// <returns></returns>
    public string ReportNickname(string url, UIDRequestData request, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <returns></returns>
    public string GetRaidTime(string url, GetRaidTimeRequest request, string sessionID)
    {
        return _httpResponseUtil.NoBody(_gameController.GetRaidTime(sessionID, request));
    }

    /// <summary>
    ///     Handle /client/survey
    /// </summary>
    /// <returns></returns>
    public string GetSurvey(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_gameController.GetSurvey(sessionID));
    }

    /// <summary>
    ///     Handle client/survey/view
    /// </summary>
    /// <returns></returns>
    public string GetSurveyView(string url, SendSurveyOpinionRequest request, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/survey/opinion
    /// </summary>
    /// <returns></returns>
    public string SendSurveyOpinion(string url, SendSurveyOpinionRequest request, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }
}
