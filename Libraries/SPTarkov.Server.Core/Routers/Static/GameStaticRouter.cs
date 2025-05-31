using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetGameConfig(url, info as GameEmptyCrcRequestData, sessionID),
                typeof(GameEmptyCrcRequestData)
            ),
            new RouteAction(
                "/client/game/mode",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetGameMode(url, info as GameModeRequestData, sessionID),
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/client/server/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetServer(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/current",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetCurrentGroup(url, info as EmptyRequestData, sessionID),
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/client/game/version/validate",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.VersionValidate(url, info as VersionValidateRequestData, sessionID),
                typeof(VersionValidateRequestData)
            ),
            new RouteAction(
                "/client/game/start",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GameStart(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/logout",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GameLogout(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/checkVersion",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.ValidateGameVersion(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/game/keepalive",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GameKeepalive(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/singleplayer/settings/version",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetVersion(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/reports/lobby/send",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.ReportNickname(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            ),
            new RouteAction(
                "/client/report/send",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.ReportNickname(url, info as UIDRequestData, sessionID),
                typeof(GameModeRequestData)
            ),
            new RouteAction(
                "/singleplayer/settings/getRaidTime",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetRaidTime(url, info as GetRaidTimeRequest, sessionID),
                typeof(GetRaidTimeRequest)
            ),
            new RouteAction(
                "/client/survey",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetSurvey(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/survey/view",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.GetSurveyView(url, info as SendSurveyOpinionRequest, sessionID),
                typeof(SendSurveyOpinionRequest)
            ),
            new RouteAction(
                "/client/survey/opinion",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await gameCallbacks.SendSurveyOpinion(url, info as SendSurveyOpinionRequest, sessionID),
                typeof(SendSurveyOpinionRequest)
            )
        ]
    )
    {
    }
}
