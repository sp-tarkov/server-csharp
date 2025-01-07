using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

public class DialogCallbacks : OnUpdate
{
    public DialogCallbacks()
    {
        
    }
    
    public GetBodyResponseData<GetFriendListDataResponse> GetFriendList(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<ChatServer>> GetChatServerList(string url, GetChatServerListRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<DialogueInfo>> GetMailDialogList(string url, GetMailDialogListRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetMailDialogViewResponseData> GetMailDialogView(string url, GetMailDialogViewRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<DialogueInfo> GetMailDialogInfo(string url, GetMailDialogInfoRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> RemoveDialog(string url, RemoveDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> PinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<List<object>> UnpinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<List<object>> SetRead(string url, SetDialogReadRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GetAllAttachments(string url, EmptyRequestData info, string sessionID) // TODO: Fix type - GetBodyResponseData<GetAllAttachmentsResponse | Undefined>
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<string> SendMessage(string url, SendMessageRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<List<object>> ListOutbox(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> ListInbox(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<FriendRequestSendResponse> SendFriendRequest(string url, FriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData AcceptAllFriendRequests(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> AcceptFriendRequest(string url, AcceptFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> DeclineFriendRequest(string url, DeclineFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<bool> CancelFriendRequest(string url, CancelFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData DeleteFriend(string url, DeleteFriendRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData IgnoreFriend(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData UnIgnoreFriend(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> ClearMail(string url, ClearMailMessageRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> RemoveMail(string url, RemoveMailMessageRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> CreateGroupMail(string url, CreateGroupMailRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> ChangeMailGroupOwner(string url, ChangeGroupMailOwnerRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> AddUserToMail(string url, AddUserGroupMailRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> RemoveUserFromMail(string url, RemoveUserGroupMailRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> OnUpdate(long timeSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}