using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Match;

namespace Core.Callbacks;

public class MatchCallbacks
{
    public NullResponseData UpdatePing(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData ExitMatch(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData ExitFromMenu(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<MatchGroupCurrentResponse> GroupCurrent(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData StartGroupSearch(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData StopGroupSearch(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<string> SendGroupInvite(string url, MatchGroupInviteSendRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<GroupCharacter>> AcceptGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> DeclineGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> CancelGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> TransferGroup(string url, MatchGroupTransferRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> CancelAllGroupInvite(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData PutMetrics(string url, PutMetricsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData EventDisconnect(string url, PutMetricsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> ServerAvailable(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<ProfileStatusResponse> JoinMatch(string url, MatchGroupStartGameRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<Metrics> GetMetrics(string url, object info, string sessionID) // TODO: No type given
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<MatchGroupStatusResponse> GetGroupStatus(string url, MatchGroupStatusRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> DeleteGroup(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> LeaveGroup(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> RemovePlayerFromGroup(string url, MatchGroupPlayerRemoveRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<StartLocalRaidResponseData> StartLocalRaid(string url, StartLocalRaidRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData EndLocalRaid(string url, EndLocalRaidRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData GetRaidConfiguration(string url, GetRaidConfigurationRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData GetConfigurationByProfile(string url, GetRaidConfigurationRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> RaidReady(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> NotRaidReady(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}