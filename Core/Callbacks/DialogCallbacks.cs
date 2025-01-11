using Core.Annotations;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.DialogCallbacks)]
public class DialogCallbacks : OnUpdate
{
    public DialogCallbacks()
    {
    }

    /// <summary>
    /// Handle client/friend/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetFriendListDataResponse> GetFriendList(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/chatServer/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<ChatServer>> GetChatServerList(string url, GetChatServerListRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<DialogueInfo>> GetMailDialogList(string url, GetMailDialogListRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/view
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetMailDialogViewResponseData> GetMailDialogView(string url, GetMailDialogViewRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/info
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<DialogueInfo> GetMailDialogInfo(string url, GetMailDialogInfoRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/remove
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<object>> RemoveDialog(string url, RemoveDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/pin
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<object>> PinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/unpin
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<object>> UnpinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/read
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<object>> SetRead(string url, SetDialogReadRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/getAllAttachments
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<object> GetAllAttachments(string url, EmptyRequestData info, string sessionID) // TODO: Fix type - GetBodyResponseData<GetAllAttachmentsResponse | Undefined>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/msg/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<string> SendMessage(string url, SendMessageRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/list/outbox
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<object>> ListOutbox(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/list/inbox
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<object>> ListInbox(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<FriendRequestSendResponse> SendFriendRequest(string url, FriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/accept-all
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData AcceptAllFriendRequests(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/accept
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<bool> AcceptFriendRequest(string url, AcceptFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/decline
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<bool> DeclineFriendRequest(string url, DeclineFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/request/cancel
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<bool> CancelFriendRequest(string url, CancelFriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/delete
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData DeleteFriend(string url, DeleteFriendRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/ignore/set
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public NullResponseData IgnoreFriend(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/ignore/remove
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
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
