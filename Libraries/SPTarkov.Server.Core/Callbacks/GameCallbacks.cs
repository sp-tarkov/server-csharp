using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.GameCallbacks)]
public class GameCallbacks(
    HttpResponseUtil httpResponseUtil,
    Watermark watermark,
    SaveServer saveServer,
    GameController gameController,
    ProfileActivityService profileActivityService,
    TimeUtil timeUtil
) : IOnLoad
{
    public Task OnLoad()
    {
        gameController.Load();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle client/game/version/validate
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> VersionValidate(string url, VersionValidateRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/game/start
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GameStart(string url, EmptyRequestData _, MongoId sessionID)
    {
        var startTimestampSec = timeUtil.GetTimeStamp();
        gameController.GameStart(url, sessionID, startTimestampSec);
        return new ValueTask<string>(httpResponseUtil.GetBody(new GameStartResponse { UtcTime = startTimestampSec }));
    }

    /// <summary>
    ///     Handle client/game/logout
    ///     Save profiles on game close
    /// </summary>
    /// <returns></returns>
    public async ValueTask<string> GameLogout(string url, EmptyRequestData _, MongoId sessionID)
    {
        await saveServer.SaveProfileAsync(sessionID);
        return httpResponseUtil.GetBody(new GameLogoutResponseData { Status = "ok" });
    }

    /// <summary>
    ///     Handle client/game/config
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetGameConfig(string url, EmptyRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetGameConfig(sessionID)));
    }

    /// <summary>
    ///     Handle client/putHWMetrics
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> PutHwMetrics(string url, EmptyRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody<string>(null!));
    }

    /// <summary>
    ///     Handle client/game/mode
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetGameMode(string url, GameModeRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetGameMode(sessionID, info)));
    }

    /// <summary>
    ///     Handle client/server/list
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetServer(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetServer(sessionID)));
    }

    /// <summary>
    ///     Handle client/match/group/current
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetCurrentGroup(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetCurrentGroup(sessionID)));
    }

    /// <summary>
    ///     Handle client/checkVersion
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ValidateGameVersion(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetValidGameVersion(sessionID)));
    }

    /// <summary>
    ///     Handle client/game/keepalive
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GameKeepalive(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetKeepAlive(sessionID)));
    }

    /// <summary>
    ///     Handle singleplayer/settings/version
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetVersion(string url, EmptyRequestData _, MongoId sessionID)
    {
        // change to be a proper type
        return new ValueTask<string>(httpResponseUtil.NoBody(new { Version = watermark.GetInGameVersionLabel() }));
    }

    /// <summary>
    ///     Handle /client/report/send and handle /client/reports/lobby/send
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ReportNickname(string url, UIDRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetRaidTime(string url, GetRaidTimeRequest request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(gameController.GetRaidTime(sessionID, request)));
    }

    /// <summary>
    ///     Handle /client/survey
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetSurvey(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(gameController.GetSurvey(sessionID)));
    }

    /// <summary>
    ///     Handle client/survey/view
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetSurveyView(string url, SendSurveyOpinionRequest request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/survey/opinion
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> SendSurveyOpinion(string url, SendSurveyOpinionRequest request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle singleplayer/clientmods
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ReceiveClientMods(string url, SendClientModsRequest request, MongoId sessionID)
    {
        profileActivityService.SetProfileActiveClientMods(sessionID, request.ActiveClientMods);

        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }
}
