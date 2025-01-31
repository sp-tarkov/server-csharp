using SptCommon.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Dialog;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnUpdate), TypePriority = OnUpdateOrder.DialogueCallbacks)]
[Injectable(InjectableTypeOverride = typeof(DialogueCallbacks))]
public class DialogueCallbacks(
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    HttpResponseUtil _httpResponseUtil,
    DialogueController _dialogueController
)
    : OnUpdate
{
    /// <summary>
    /// Handle client/friend/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetFriendList(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GetFriendList(sessionID));
    }

    /// <summary>
    /// Handle client/chatServer/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetChatServerList(string url, GetChatServerListRequestData info, string sessionID)
    {
        var chatServer = new List<ChatServer>
        {
            new ChatServer
            {
                Id = _hashUtil.Generate(),
                RegistrationId = 20,
                DateTime = _timeUtil.GetTimeStamp(),
                IsDeveloper = true,
                Regions = ["EUR"],
                VersionId = "bgkidft87ddd",
                Ip = "",
                Port = 0,
                Chats = [ new Chat { Id = "0", Members = 0 } ],
            }
        };

        return _httpResponseUtil.GetBody(chatServer);
    }

    /// <summary>
    /// Handle client/mail/dialog/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetMailDialogList(string url, GetMailDialogListRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GenerateDialogueList(sessionID), 0, null, false);
    }

    /// <summary>
    /// Handle client/mail/dialog/view
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetMailDialogView(string url, GetMailDialogViewRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GenerateDialogueView(info, sessionID), 0, null, false);
    }

    /// <summary>
    /// Handle client/mail/dialog/info
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetMailDialogInfo(string url, GetMailDialogInfoRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GetDialogueInfo(info.DialogId, sessionID));
    }

    /// <summary>
    /// Handle client/mail/dialog/remove
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string RemoveDialog(string url, RemoveDialogRequestData info, string sessionID)
    {
        _dialogueController.RemoveDialogue(info.DialogId, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/mail/dialog/pin
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string PinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        _dialogueController.SetDialoguePin(info.DialogId, true, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/mail/dialog/unpin
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string UnpinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        _dialogueController.SetDialoguePin(info.DialogId, false, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/mail/dialog/read
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SetRead(string url, SetDialogReadRequestData info, string sessionID)
    {
        _dialogueController.SetRead(info.Dialogs, sessionID);
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/mail/dialog/getAllAttachments
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetAllAttachments(string url, GetAllAttachmentsRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.GetAllAttachments(info.DialogId, sessionID));
    }

    /// <summary>
    /// Handle client/mail/msg/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SendMessage(string url, SendMessageRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.SendMessage(sessionID, info));
    }

    /// <summary>
    /// Handle client/friend/request/list/outbox
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ListOutbox(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/friend/request/list/inbox
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ListInbox(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/friend/request/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SendFriendRequest(string url, FriendRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_dialogueController.SendFriendRequest(sessionID, info));
    }

    /// <summary>
    /// Handle client/friend/request/accept-all
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string AcceptAllFriendRequests(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/friend/request/accept
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string AcceptFriendRequest(string url, AcceptFriendRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/friend/request/decline
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string DeclineFriendRequest(string url, DeclineFriendRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/friend/request/cancel
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string CancelFriendRequest(string url, CancelFriendRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/friend/delete
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string DeleteFriend(string url, DeleteFriendRequest info, string sessionID)
    {
        _dialogueController.DeleteFriend(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/friend/ignore/set
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string IgnoreFriend(string url, UIDRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/friend/ignore/remove
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string UnIgnoreFriend(string url, UIDRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    public string ClearMail(string url, ClearMailMessageRequest info, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    public string CreateGroupMail(string url, CreateGroupMailRequest info, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    public string ChangeMailGroupOwner(string url, ChangeGroupMailOwnerRequest info, string sessionID)
    {
        return "Not Implemented!"; // Not implemented in Node
    }

    public string AddUserToMail(string url, AddUserGroupMailRequest info, string sessionID)
    {
        return "Not Implemented!"; // Not implemented in Node
    }

    public string RemoveUserFromMail(string url, RemoveUserGroupMailRequest info, string sessionID)
    {
        return "Not Implemented!"; // Not implemented in Node
    }

    public bool OnUpdate(long timeSinceLastRun)
    {
        _dialogueController.Update();
        return true;
    }

    public string GetRoute()
    {
        return "spt-dialogue";
    }
}
