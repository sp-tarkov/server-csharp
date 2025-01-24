using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Match;
using Core.Utils;
using static Core.Services.MatchLocationService;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class MatchStaticRouter : StaticRouter
{
    protected static MatchCallbacks _matchCallbacks;
    
    public MatchStaticRouter(
        JsonUtil jsonUtil,
        MatchCallbacks matchCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/match/available",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.ServerAvailable(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/updatePing",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.UpdatePing(url, info as UpdatePingRequestData, sessionID),
                typeof(UpdatePingRequestData)),
            new RouteAction(
                "/client/match/join",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.JoinMatch(url, info as MatchGroupStartGameRequest, sessionID),
                typeof(MatchGroupStartGameRequest)),
            new RouteAction(
                "/client/match/exit",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.ExitMatch(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/delete",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.DeleteGroup(url, info as DeleteGroupRequest, sessionID)),
            new RouteAction(
                "/client/match/group/leave",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.LeaveGroup(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/status",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.GetGroupStatus(url, info as MatchGroupStatusRequest, sessionID),
                typeof(MatchGroupStatusRequest)),
            new RouteAction(
                "/client/match/group/start_game",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.JoinMatch(url, info as MatchGroupStartGameRequest, sessionID),
                typeof(MatchGroupStartGameRequest)),
            new RouteAction(
                "/client/match/group/exit_from_menu",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.ExitFromMenu(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/current",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.GroupCurrent(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/looking/start",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.StartGroupSearch(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/looking/stop",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.StopGroupSearch(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/invite/send",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.SendGroupInvite(url, info as MatchGroupInviteSendRequest, sessionID),
                typeof(MatchGroupInviteSendRequest)),
            new RouteAction(
                "/client/match/group/invite/accept",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.AcceptGroupInvite(url, info as RequestIdRequest, sessionID),
                typeof(RequestIdRequest)),
            new RouteAction(
                "/client/match/group/invite/decline",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.DeclineGroupInvite(url, info as RequestIdRequest, sessionID),
                typeof(RequestIdRequest)),
            new RouteAction(
                "/client/match/group/invite/cancel",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.CancelGroupInvite(url, info as RequestIdRequest, sessionID),
                typeof(RequestIdRequest)),
            new RouteAction(
                "/client/match/group/invite/cancel-all",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.CancelAllGroupInvite(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/transfer",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.TransferGroup(url, info as MatchGroupTransferRequest, sessionID),
                typeof(MatchGroupTransferRequest)),
            new RouteAction(
                "/client/match/group/raid/ready",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.RaidReady(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/match/group/raid/not-ready",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.NotRaidReady(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/putMetrics",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.PutMetrics(url, info as PutMetricsRequestData, sessionID),
                typeof(PutMetricsRequestData)),
            new RouteAction(
                "/client/analytics/event-disconnect",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.EventDisconnect(url, info as PutMetricsRequestData, sessionID),
                typeof(PutMetricsRequestData)),
            new RouteAction(
                "/client/getMetricsConfig",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.GetMetrics(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "/client/raid/configuration",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.GetRaidConfiguration(url, info as GetRaidConfigurationRequestData, sessionID),
                typeof(GetRaidConfigurationRequestData)),
            new RouteAction(
                "/client/raid/configuration-by-profile",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.GetConfigurationByProfile(url, info as GetRaidConfigurationRequestData, sessionID),
                typeof(GetRaidConfigurationRequestData)),
            new RouteAction(
                "/client/match/group/player/remove",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.RemovePlayerFromGroup(url, info as MatchGroupPlayerRemoveRequest, sessionID),
                typeof(MatchGroupPlayerRemoveRequest)),
            new RouteAction(
                "/client/match/local/start",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.StartLocalRaid(url, info as StartLocalRaidRequestData, sessionID),
                typeof(StartLocalRaidRequestData)),
            new RouteAction(
                "/client/match/local/end",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _matchCallbacks.EndLocalRaid(url, info as EndLocalRaidRequestData, sessionID),
                typeof(EndLocalRaidRequestData))
        ]
    )
    {
        _matchCallbacks = matchCallbacks;
    }
}
