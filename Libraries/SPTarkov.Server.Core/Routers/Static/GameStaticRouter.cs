using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class GameStaticRouter(JsonUtil jsonUtil, GameCallbacks gameCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/game/config",
                async (url, info, sessionID, output) => await gameCallbacks.GetGameConfig(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/putHWMetrics",
                async (url, info, sessionID, output) => await gameCallbacks.PutHwMetrics(url, info, sessionID)
            ),
            new RouteAction<GameModeRequestData>(
                "/client/game/mode",
                async (url, info, sessionID, output) => await gameCallbacks.GetGameMode(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/server/list",
                async (url, info, sessionID, output) => await gameCallbacks.GetServer(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/current",
                async (url, info, sessionID, output) => await gameCallbacks.GetCurrentGroup(url, info, sessionID)
            ),
            new RouteAction<VersionValidateRequestData>(
                "/client/game/version/validate",
                async (url, info, sessionID, output) => await gameCallbacks.VersionValidate(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/game/start",
                async (url, info, sessionID, output) => await gameCallbacks.GameStart(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/game/logout",
                async (url, info, sessionID, output) => await gameCallbacks.GameLogout(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/checkVersion",
                async (url, info, sessionID, output) => await gameCallbacks.ValidateGameVersion(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/game/keepalive",
                async (url, info, sessionID, output) => await gameCallbacks.GameKeepalive(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/settings/version",
                async (url, info, sessionID, output) => await gameCallbacks.GetVersion(url, info, sessionID)
            ),
            new RouteAction<UIDRequestData>(
                "/client/reports/lobby/send",
                async (url, info, sessionID, output) => await gameCallbacks.ReportNickname(url, info, sessionID)
            ),
            new RouteAction<UIDRequestData>(
                "/client/report/send",
                async (url, info, sessionID, output) => await gameCallbacks.ReportNickname(url, info, sessionID)
            ),
            new RouteAction<GetRaidTimeRequest>(
                "/singleplayer/settings/getRaidTime",
                async (url, info, sessionID, output) => await gameCallbacks.GetRaidTime(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/survey",
                async (url, info, sessionID, output) => await gameCallbacks.GetSurvey(url, info, sessionID)
            ),
            new RouteAction<SendSurveyOpinionRequest>(
                "/client/survey/view",
                async (url, info, sessionID, output) => await gameCallbacks.GetSurveyView(url, info, sessionID)
            ),
            new RouteAction<SendSurveyOpinionRequest>(
                "/client/survey/opinion",
                async (url, info, sessionID, output) => await gameCallbacks.SendSurveyOpinion(url, info, sessionID)
            ),
            new RouteAction<SendClientModsRequest>(
                "/singleplayer/clientmods",
                async (url, info, sessionID, output) => await gameCallbacks.ReceiveClientMods(url, info, sessionID)
            ),
        ]
    ) { }
