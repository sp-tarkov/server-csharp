using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;
using static SPTarkov.Server.Core.Services.MatchLocationService;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class MatchStaticRouter : StaticRouter
{
    public MatchStaticRouter(
        JsonUtil jsonUtil,
        MatchCallbacks matchCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/match/available",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.ServerAvailable(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/updatePing",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.UpdatePing(url, info as UpdatePingRequestData, sessionID),
                typeof(UpdatePingRequestData)
            ),
            new RouteAction(
                "/client/match/join",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.JoinMatch(url, info as MatchGroupStartGameRequest, sessionID),
                typeof(MatchGroupStartGameRequest)
            ),
            new RouteAction(
                "/client/match/exit",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.ExitMatch(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/delete",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.DeleteGroup(url, info as DeleteGroupRequest, sessionID)
            ),
            new RouteAction(
                "/client/match/group/leave",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.LeaveGroup(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/status",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.GetGroupStatus(url, info as MatchGroupStatusRequest, sessionID),
                typeof(MatchGroupStatusRequest)
            ),
            new RouteAction(
                "/client/match/group/start_game",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.JoinMatch(url, info as MatchGroupStartGameRequest, sessionID),
                typeof(MatchGroupStartGameRequest)
            ),
            new RouteAction(
                "/client/match/group/exit_from_menu",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.ExitFromMenu(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/current",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.GroupCurrent(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/looking/start",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.StartGroupSearch(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/looking/stop",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.StopGroupSearch(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/invite/send",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.SendGroupInvite(url, info as MatchGroupInviteSendRequest, sessionID),
                typeof(MatchGroupInviteSendRequest)
            ),
            new RouteAction(
                "/client/match/group/invite/accept",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.AcceptGroupInvite(url, info as RequestIdRequest, sessionID),
                typeof(RequestIdRequest)
            ),
            new RouteAction(
                "/client/match/group/invite/decline",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.DeclineGroupInvite(url, info as RequestIdRequest, sessionID),
                typeof(RequestIdRequest)
            ),
            new RouteAction(
                "/client/match/group/invite/cancel",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.CancelGroupInvite(url, info as RequestIdRequest, sessionID),
                typeof(RequestIdRequest)
            ),
            new RouteAction(
                "/client/match/group/invite/cancel-all",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.CancelAllGroupInvite(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/transfer",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.TransferGroup(url, info as MatchGroupTransferRequest, sessionID),
                typeof(MatchGroupTransferRequest)
            ),
            new RouteAction(
                "/client/match/group/raid/ready",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.RaidReady(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/match/group/raid/not-ready",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.NotRaidReady(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/putMetrics",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.PutMetrics(url, info as PutMetricsRequestData, sessionID),
                typeof(PutMetricsRequestData)
            ),
            new RouteAction(
                "/client/analytics/event-disconnect",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.EventDisconnect(url, info as PutMetricsRequestData, sessionID),
                typeof(PutMetricsRequestData)
            ),
            new RouteAction(
                "/client/getMetricsConfig",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.GetMetrics(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/raid/configuration",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.GetRaidConfiguration(url, info as GetRaidConfigurationRequestData, sessionID),
                typeof(GetRaidConfigurationRequestData)
            ),
            new RouteAction(
                "/client/raid/configuration-by-profile",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.GetConfigurationByProfile(url, info as GetRaidConfigurationRequestData, sessionID),
                typeof(GetRaidConfigurationRequestData)
            ),
            new RouteAction(
                "/client/match/group/player/remove",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.RemovePlayerFromGroup(url, info as MatchGroupPlayerRemoveRequest, sessionID),
                typeof(MatchGroupPlayerRemoveRequest)
            ),
            new RouteAction(
                "/client/match/local/start",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.StartLocalRaid(url, info as StartLocalRaidRequestData, sessionID),
                typeof(StartLocalRaidRequestData)
            ),
            new RouteAction(
                "/client/match/local/end",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await matchCallbacks.EndLocalRaid(url, info as EndLocalRaidRequestData, sessionID),
                typeof(EndLocalRaidRequestData)
            )
        ]
    )
    {
    }
}
