using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Game;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class GameStaticRouter : StaticRouter
{
    public GameStaticRouter(
        JsonUtil jsonUtil,
        GameCallbacks gameCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/game/config",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetGameConfig(url, info as GameEmptyCrcRequestData, sessionID),
                typeof(GameEmptyCrcRequestData)
            ),
            new RouteAction(
                "/client/game/mode",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetGameMode(url, info as GameModeRequestData, sessionID),
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/client/server/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetServer(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/current",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetCurrentGroup(url, info as EmptyRequestData, sessionID),
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/client/game/version/validate",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.VersionValidate(url, info as VersionValidateRequestData, sessionID),
                typeof(VersionValidateRequestData)
            ),
            new RouteAction(
                "/client/game/start",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GameStart(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/logout",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GameLogout(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/checkVersion",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.ValidateGameVersion(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/keepalive",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GameKeepalive(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/version",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetVersion(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/reports/lobby/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.ReportNickname(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            ),
            new RouteAction(
                "/client/report/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.ReportNickname(url, info as UIDRequestData, sessionID),
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/singleplayer/settings/getRaidTime",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetRaidTime(url, info as GetRaidTimeRequest, sessionID),
                typeof(GetRaidTimeRequest)
            ),
            new RouteAction(
                "/client/survey",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetSurvey(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/survey/view",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.GetSurveyView(url, info as SendSurveyOpinionRequest, sessionID),
                typeof(SendSurveyOpinionRequest)
            ),
            new RouteAction(
                "/client/survey/opinion",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => gameCallbacks.SendSurveyOpinion(url, info as SendSurveyOpinionRequest, sessionID),
                typeof(SendSurveyOpinionRequest)
            )
        ]
    )
    {
    }
}
