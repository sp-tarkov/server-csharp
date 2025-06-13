using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.GameCallbacks)]
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

    /// <summary>
    ///     Handle client/game/version/validate
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> VersionValidate(string url, VersionValidateRequestData info, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/game/start
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GameStart(string url, EmptyRequestData _, string sessionID)
    {
        var startTimestampSec = _timeUtil.GetTimeStamp();
        _gameController.GameStart(url, sessionID, startTimestampSec);
        return new ValueTask<string>(_httpResponseUtil.GetBody(
            new GameStartResponse
            {
                UtcTime = startTimestampSec
            }
        ));
    }

    /// <summary>
    ///     Handle client/game/logout
    ///     Save profiles on game close
    /// </summary>
    /// <returns></returns>
    public async ValueTask<string> GameLogout(string url, EmptyRequestData _, string sessionID)
    {
        await _saveServer.SaveProfileAsync(sessionID);
        return _httpResponseUtil.GetBody(
            new GameLogoutResponseData
            {
                Status = "ok"
            }
        );
    }

    /// <summary>
    ///     Handle client/game/config
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetGameConfig(string url, GameEmptyCrcRequestData info, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetGameConfig(sessionID)));
    }

    /// <summary>
    ///     Handle client/game/mode
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetGameMode(string url, GameModeRequestData info, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetGameMode(sessionID, info)));
    }

    /// <summary>
    ///     Handle client/server/list
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetServer(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetServer(sessionID)));
    }

    /// <summary>
    ///     Handle client/match/group/current
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetCurrentGroup(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetCurrentGroup(sessionID)));
    }

    /// <summary>
    ///     Handle client/checkVersion
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ValidateGameVersion(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetValidGameVersion(sessionID)));
    }

    /// <summary>
    ///     Handle client/game/keepalive
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GameKeepalive(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetKeepAlive(sessionID)));
    }

    /// <summary>
    ///     Handle singleplayer/settings/version
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetVersion(string url, EmptyRequestData _, string sessionID)
    {
        // change to be a proper type
        return new ValueTask<string>(_httpResponseUtil.NoBody(
            new
            {
                Version = _watermark.GetInGameVersionLabel()
            }
        ));
    }

    /// <summary>
    ///     Handle /client/report/send & /client/reports/lobby/send
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ReportNickname(string url, UIDRequestData request, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetRaidTime(string url, GetRaidTimeRequest request, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_gameController.GetRaidTime(sessionID, request)));
    }

    /// <summary>
    ///     Handle /client/survey
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetSurvey(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_gameController.GetSurvey(sessionID)));
    }

    /// <summary>
    ///     Handle client/survey/view
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetSurveyView(string url, SendSurveyOpinionRequest request, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/survey/opinion
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> SendSurveyOpinion(string url, SendSurveyOpinionRequest request, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }
}
