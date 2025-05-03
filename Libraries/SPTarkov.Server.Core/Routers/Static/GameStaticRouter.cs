using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

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
                ) => { return gameCallbacks.GetGameConfig(url, info as GameEmptyCrcRequestData, sessionID); },
                typeof(GameEmptyCrcRequestData)
            ),
            new RouteAction(
                "/client/game/mode",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetGameMode(url, info as GameModeRequestData, sessionID); },
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/client/server/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetServer(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/match/group/current",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetCurrentGroup(url, info as EmptyRequestData, sessionID); },
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/client/game/version/validate",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.VersionValidate(url, info as VersionValidateRequestData, sessionID); },
                typeof(VersionValidateRequestData)
            ),
            new RouteAction(
                "/client/game/start",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GameStart(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/game/logout",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GameLogout(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/checkVersion",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.ValidateGameVersion(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/game/keepalive",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GameKeepalive(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/singleplayer/settings/version",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetVersion(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/reports/lobby/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.ReportNickname(url, info as UIDRequestData, sessionID); },
                typeof(UIDRequestData)
            ),
            new RouteAction(
                "/client/report/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.ReportNickname(url, info as UIDRequestData, sessionID); },
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/singleplayer/settings/getRaidTime",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetRaidTime(url, info as GetRaidTimeRequest, sessionID); },
                typeof(GetRaidTimeRequest)
            ),
            new RouteAction(
                "/client/survey",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetSurvey(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/survey/view",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.GetSurveyView(url, info as SendSurveyOpinionRequest, sessionID); },
                typeof(SendSurveyOpinionRequest)
            ),
            new RouteAction(
                "/client/survey/opinion",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return gameCallbacks.SendSurveyOpinion(url, info as SendSurveyOpinionRequest, sessionID); },
                typeof(SendSurveyOpinionRequest)
            )
        ]
    )
    {
    }
}
