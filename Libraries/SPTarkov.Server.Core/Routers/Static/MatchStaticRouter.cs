using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;
using static SPTarkov.Server.Core.Services.MatchLocationService;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class MatchStaticRouter(JsonUtil jsonUtil, MatchCallbacks matchCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/match/available",
                async (url, info, sessionID, output) => await matchCallbacks.ServerAvailable(url, info, sessionID)
            ),
            new RouteAction<UpdatePingRequestData>(
                "/client/match/updatePing",
                async (url, info, sessionID, output) => await matchCallbacks.UpdatePing(url, info, sessionID)
            ),
            new RouteAction<MatchGroupJoinRequest>(
                "/client/match/join",
                async (url, info, sessionID, output) => await matchCallbacks.JoinMatch(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/exit",
                async (url, info, sessionID, output) => await matchCallbacks.ExitMatch(url, info, sessionID)
            ),
            new RouteAction<DeleteGroupRequest>(
                "/client/match/group/delete",
                async (url, info, sessionID, output) => await matchCallbacks.DeleteGroup(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/leave",
                async (url, info, sessionID, output) => await matchCallbacks.LeaveGroup(url, info, sessionID)
            ),
            new RouteAction<MatchGroupStatusRequest>(
                "/client/match/group/status",
                async (url, info, sessionID, output) => await matchCallbacks.GetGroupStatus(url, info, sessionID)
            ),
            new RouteAction<MatchGroupStartGameRequest>(
                "/client/match/group/start_game",
                async (url, info, sessionID, output) => await matchCallbacks.StartGameAsGroupLeader(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/exit_from_menu",
                async (url, info, sessionID, output) => await matchCallbacks.ExitFromMenu(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/current",
                async (url, info, sessionID, output) => await matchCallbacks.GroupCurrent(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/looking/start",
                async (url, info, sessionID, output) => await matchCallbacks.StartGroupSearch(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/looking/stop",
                async (url, info, sessionID, output) => await matchCallbacks.StopGroupSearch(url, info, sessionID)
            ),
            new RouteAction<MatchGroupInviteSendRequest>(
                "/client/match/group/invite/send",
                async (url, info, sessionID, output) => await matchCallbacks.SendGroupInvite(url, info, sessionID)
            ),
            new RouteAction<RequestIdRequest>(
                "/client/match/group/invite/accept",
                async (url, info, sessionID, output) => await matchCallbacks.AcceptGroupInvite(url, info, sessionID)
            ),
            new RouteAction<RequestIdRequest>(
                "/client/match/group/invite/decline",
                async (url, info, sessionID, output) => await matchCallbacks.DeclineGroupInvite(url, info, sessionID)
            ),
            new RouteAction<RequestIdRequest>(
                "/client/match/group/invite/cancel",
                async (url, info, sessionID, output) => await matchCallbacks.CancelGroupInvite(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/invite/cancel-all",
                async (url, info, sessionID, output) => await matchCallbacks.CancelAllGroupInvite(url, info, sessionID)
            ),
            new RouteAction<MatchGroupTransferRequest>(
                "/client/match/group/transfer",
                async (url, info, sessionID, output) => await matchCallbacks.TransferGroup(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/raid/ready",
                async (url, info, sessionID, output) => await matchCallbacks.RaidReady(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/match/group/raid/not-ready",
                async (url, info, sessionID, output) => await matchCallbacks.NotRaidReady(url, info, sessionID)
            ),
            new RouteAction<PutMetricsRequestData>(
                "/client/putMetrics",
                async (url, info, sessionID, output) => await matchCallbacks.PutMetrics(url, info, sessionID)
            ),
            new RouteAction<PutMetricsRequestData>(
                "/client/analytics/event-disconnect",
                async (url, info, sessionID, output) => await matchCallbacks.EventDisconnect(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/getMetricsConfig",
                async (url, info, sessionID, output) => await matchCallbacks.GetMetrics(url, info, sessionID)
            ),
            new RouteAction<GetRaidConfigurationRequestData>(
                "/client/raid/configuration",
                async (url, info, sessionID, output) => await matchCallbacks.GetRaidConfiguration(url, info, sessionID)
            ),
            new RouteAction<GetRaidConfigurationRequestData>(
                "/client/raid/configuration-by-profile",
                async (url, info, sessionID, output) => await matchCallbacks.GetConfigurationByProfile(url, info, sessionID)
            ),
            new RouteAction<MatchGroupPlayerRemoveRequest>(
                "/client/match/group/player/remove",
                async (url, info, sessionID, output) => await matchCallbacks.RemovePlayerFromGroup(url, info, sessionID)
            ),
            new RouteAction<StartLocalRaidRequestData>(
                "/client/match/local/start",
                async (url, info, sessionID, output) => await matchCallbacks.StartLocalRaid(url, info, sessionID)
            ),
            new RouteAction<EndLocalRaidRequestData>(
                "/client/match/local/end",
                async (url, info, sessionID, output) => await matchCallbacks.EndLocalRaid(url, info, sessionID)
            ),
        ]
    ) { }
