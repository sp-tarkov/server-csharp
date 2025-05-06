using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.DialogueCallbacks)]
public class DialogueCallbacks(
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    HttpResponseUtil _httpResponseUtil,
    DialogueController _dialogueController
)
    : IOnUpdate
{
    public bool OnUpdate(long timeSinceLastRun)
    {
        _dialogueController.Update();
        return true;
    }

    public string GetRoute()
    {
        return "spt-dialogue";
    }

    /// <summary>
    ///     Handle client/friend/list
    /// </summary>
    /// <returns></returns>
    public virtual string GetFriendList(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GetFriendList(sessionID));
    }

    /// <summary>
    ///     Handle client/chatServer/list
    /// </summary>
    /// <returns></returns>
    public virtual string GetChatServerList(string url, GetChatServerListRequestData request, string sessionID)
    {
        var chatServer = new List<ChatServer>
        {
            new()
            {
                Id = _hashUtil.Generate(),
                RegistrationId = 20,
                DateTime = _timeUtil.GetTimeStamp(),
                IsDeveloper = true,
                Regions = ["EUR"],
                VersionId = "bgkidft87ddd",
                Ip = "",
                Port = 0,
                Chats =
                [
                    new Chat
                    {
                        Id = "0",
                        Members = 0
                    }
                ]
            }
        };

        return _httpResponseUtil.GetBody(chatServer);
    }

    /// <summary>
    ///     Handle client/mail/dialog/list
    ///     TODO: request properties are not handled
    /// </summary>
    /// <returns></returns>
    public virtual string GetMailDialogList(string url, GetMailDialogListRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GenerateDialogueList(sessionID), 0, null, false);
    }

    /// <summary>
    ///     Handle client/mail/dialog/view
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public virtual string GetMailDialogView(string url, GetMailDialogViewRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GenerateDialogueView(request, sessionID), 0, null, false);
    }

    /// <summary>
    ///     Handle client/mail/dialog/info
    /// </summary>
    /// <returns></returns>
    public virtual string GetMailDialogInfo(string url, GetMailDialogInfoRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GetDialogueInfo(request.DialogId, sessionID));
    }

    /// <summary>
    ///     Handle client/mail/dialog/remove
    /// </summary>
    /// <returns></returns>
    public virtual string RemoveDialog(string url, RemoveDialogRequestData request, string sessionID)
    {
        _dialogueController.RemoveDialogue(request.DialogId, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/mail/dialog/pin
    /// </summary>
    /// <returns></returns>
    public virtual string PinDialog(string url, PinDialogRequestData request, string sessionID)
    {
        _dialogueController.SetDialoguePin(request.DialogId, true, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/mail/dialog/unpin
    /// </summary>
    /// <returns></returns>
    public virtual string UnpinDialog(string url, PinDialogRequestData request, string sessionID)
    {
        _dialogueController.SetDialoguePin(request.DialogId, false, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/mail/dialog/read
    /// </summary>
    /// <returns></returns>
    public virtual string SetRead(string url, SetDialogReadRequestData request, string sessionID)
    {
        _dialogueController.SetRead(request.Dialogs, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/mail/dialog/getAllAttachments
    /// </summary>
    /// <returns></returns>
    public virtual string GetAllAttachments(string url, GetAllAttachmentsRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GetAllAttachments(request.DialogId, sessionID));
    }

    /// <summary>
    ///     Handle client/mail/msg/send
    /// </summary>
    /// <returns></returns>
    public virtual string SendMessage(string url, SendMessageRequest request, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.SendMessage(sessionID, request));
    }

    /// <summary>
    ///     Handle client/friend/request/list/outbox
    /// </summary>
    /// <returns></returns>
    public virtual string ListOutbox(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/friend/request/list/inbox
    /// </summary>
    /// <returns></returns>
    public virtual string ListInbox(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/friend/request/send
    /// </summary>
    /// <returns></returns>
    public virtual string SendFriendRequest(string url, FriendRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.SendFriendRequest(sessionID, request));
    }

    /// <summary>
    ///     Handle client/friend/request/accept-all
    /// </summary>
    /// <returns></returns>
    public virtual string AcceptAllFriendRequests(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/friend/request/accept
    /// </summary>
    /// <returns></returns>
    public virtual string AcceptFriendRequest(string url, AcceptFriendRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/friend/request/decline
    /// </summary>
    /// <returns></returns>
    public virtual string DeclineFriendRequest(string url, DeclineFriendRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/friend/request/cancel
    /// </summary>
    /// <returns></returns>
    public virtual string CancelFriendRequest(string url, CancelFriendRequestData request, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/friend/delete
    /// </summary>
    /// <returns></returns>
    public virtual string DeleteFriend(string url, DeleteFriendRequest request, string sessionID)
    {
        _dialogueController.DeleteFriend(sessionID, request);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/friend/ignore/set
    /// </summary>
    /// <returns></returns>
    public virtual string IgnoreFriend(string url, UIDRequestData request, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/friend/ignore/remove
    /// </summary>
    /// <returns></returns>
    public virtual string UnIgnoreFriend(string url, UIDRequestData request, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    public virtual string ClearMail(string url, ClearMailMessageRequest request, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    public virtual string CreateGroupMail(string url, CreateGroupMailRequest request, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    public virtual string ChangeMailGroupOwner(string url, ChangeGroupMailOwnerRequest request, string sessionID)
    {
        return "Not Implemented!"; // Not implemented in Node
    }

    public virtual string AddUserToMail(string url, AddUserGroupMailRequest request, string sessionID)
    {
        return "Not Implemented!"; // Not implemented in Node
    }

    public virtual string RemoveUserFromMail(string url, RemoveUserGroupMailRequest request, string sessionID)
    {
        return "Not Implemented!"; // Not implemented in Node
    }
}
