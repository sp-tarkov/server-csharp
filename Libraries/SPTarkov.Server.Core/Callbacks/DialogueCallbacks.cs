using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.DialogueCallbacks)]
public class DialogueCallbacks(TimeUtil timeUtil, HttpResponseUtil httpResponseUtil, DialogueController dialogueController) : IOnUpdate
{
    public Task<bool> OnUpdate(long timeSinceLastRun)
    {
        dialogueController.Update();
        return Task.FromResult(true);
    }

    /// <summary>
    ///     Handle client/friend/list
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> GetFriendList(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(dialogueController.GetFriendList(sessionID)));
    }

    /// <summary>
    ///     Handle client/chatServer/list
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> GetChatServerList(string url, GetChatServerListRequestData request, MongoId sessionID)
    {
        var chatServer = new List<ChatServer>
        {
            new()
            {
                Id = new MongoId(),
                RegistrationId = 20,
                DateTime = timeUtil.GetTimeStamp(),
                IsDeveloper = true,
                Regions = ["EUR"],
                VersionId = request.VersionId,
                Ip = "",
                Port = 0,
                Chats = [new Chat { Id = "0", Members = 0 }],
            },
        };

        return new ValueTask<string>(httpResponseUtil.GetBody(chatServer));
    }

    /// <summary>
    ///     Handle client/mail/dialog/list
    ///     TODO: request properties are not handled
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> GetMailDialogList(string url, GetMailDialogListRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(dialogueController.GenerateDialogueList(sessionID), 0, null, false));
    }

    /// <summary>
    ///     Handle client/mail/dialog/view
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public virtual ValueTask<string> GetMailDialogView(string url, GetMailDialogViewRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(dialogueController.GenerateDialogueView(request, sessionID), 0, null, false));
    }

    /// <summary>
    ///     Handle client/mail/dialog/info
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> GetMailDialogInfo(string url, GetMailDialogInfoRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(
            httpResponseUtil.GetBody(dialogueController.GetDialogueInfo(request.DialogId ?? MongoId.Empty(), sessionID))
        );
    }

    /// <summary>
    ///     Handle client/mail/dialog/remove
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> RemoveDialog(string url, RemoveDialogRequestData request, MongoId sessionID)
    {
        dialogueController.RemoveDialogue(request.DialogId ?? MongoId.Empty(), sessionID);
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle client/mail/dialog/pin
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> PinDialog(string url, PinDialogRequestData request, MongoId sessionID)
    {
        dialogueController.SetDialoguePin(request.DialogId ?? MongoId.Empty(), true, sessionID);
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle client/mail/dialog/unpin
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> UnpinDialog(string url, PinDialogRequestData request, MongoId sessionID)
    {
        dialogueController.SetDialoguePin(request.DialogId ?? MongoId.Empty(), false, sessionID);
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle client/mail/dialog/read
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> SetRead(string url, SetDialogReadRequestData request, MongoId sessionID)
    {
        dialogueController.SetRead(request.Dialogs, sessionID);
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle client/mail/dialog/getAllAttachments
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> GetAllAttachments(string url, GetAllAttachmentsRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(dialogueController.GetAllAttachments(request.DialogId, sessionID)));
    }

    /// <summary>
    ///     Handle client/mail/msg/send
    /// </summary>
    /// <returns></returns>
    public virtual async ValueTask<string> SendMessage(string url, SendMessageRequest request, MongoId sessionID)
    {
        return httpResponseUtil.GetBody(await dialogueController.SendMessage(sessionID, request));
    }

    /// <summary>
    ///     Handle client/friend/request/list/outbox
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> ListOutbox(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle client/friend/request/list/inbox
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> ListInbox(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle client/friend/request/send
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> SendFriendRequest(string url, FriendRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(dialogueController.SendFriendRequest(sessionID, request)));
    }

    /// <summary>
    ///     Handle client/friend/request/accept-all
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> AcceptAllFriendRequests(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/friend/request/accept
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> AcceptFriendRequest(string url, AcceptFriendRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(true));
    }

    /// <summary>
    ///     Handle client/friend/request/decline
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> DeclineFriendRequest(string url, DeclineFriendRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(true));
    }

    /// <summary>
    ///     Handle client/friend/request/cancel
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> CancelFriendRequest(string url, CancelFriendRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(true));
    }

    /// <summary>
    ///     Handle client/friend/delete
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> DeleteFriend(string url, DeleteFriendRequest request, MongoId sessionID)
    {
        dialogueController.DeleteFriend(sessionID, request);
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/friend/ignore/set
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> IgnoreFriend(string url, UIDRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/friend/ignore/remove
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> UnIgnoreFriend(string url, UIDRequestData request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle /client/mail/dialog/clear
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> ClearMail(string url, ClearMailMessageRequest request, MongoId sessionID)
    {
        dialogueController.ClearMessages(sessionID, request);

        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle /client/mail/dialog/group/create
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> CreateGroupMail(string url, CreateGroupMailRequest request, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.EmptyArrayResponse());
    }

    /// <summary>
    ///     Handle /client/mail/dialog/group/owner/change
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> ChangeMailGroupOwner(string url, ChangeGroupMailOwnerRequest request, MongoId sessionID)
    {
        return new ValueTask<string>("Not Implemented!"); // Not implemented in Node
    }

    /// <summary>
    ///     Handle /client/mail/dialog/group/users/add
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> AddUserToMail(string url, AddUserGroupMailRequest request, MongoId sessionID)
    {
        return new ValueTask<string>("Not Implemented!"); // Not implemented in Node
    }

    /// <summary>
    ///     Handle /client/mail/dialog/group/users/remove
    /// </summary>
    /// <returns></returns>
    public virtual ValueTask<string> RemoveUserFromMail(string url, RemoveUserGroupMailRequest request, MongoId sessionID)
    {
        return new ValueTask<string>("Not Implemented!"); // Not implemented in Node
    }
}
