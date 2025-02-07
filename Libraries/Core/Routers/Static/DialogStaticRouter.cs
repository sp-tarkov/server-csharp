using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Dialog;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class DialogStaticRouter : StaticRouter
{
    protected static DialogueCallbacks _dialogueCallbacks;

    public DialogStaticRouter(
        JsonUtil jsonUtil,
        DialogueCallbacks dialogueCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/chatServer/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.GetChatServerList(url, info as GetChatServerListRequestData, sessionID),
                typeof(GetChatServerListRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.GetMailDialogList(url, info as GetMailDialogListRequestData, sessionID),
                typeof(GetMailDialogListRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/view",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.GetMailDialogView(url, info as GetMailDialogViewRequestData, sessionID),
                typeof(GetMailDialogViewRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/info",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.GetMailDialogInfo(url, info as GetMailDialogInfoRequestData, sessionID),
                typeof(GetMailDialogInfoRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/remove",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.RemoveDialog(url, info as RemoveDialogRequestData, sessionID),
                typeof(RemoveDialogRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/pin",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.PinDialog(url, info as PinDialogRequestData, sessionID),
                typeof(PinDialogRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/unpin",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.UnpinDialog(url, info as PinDialogRequestData, sessionID),
                typeof(PinDialogRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/read",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.SetRead(url, info as SetDialogReadRequestData, sessionID),
                typeof(SetDialogReadRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/getAllAttachments",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.GetAllAttachments(url, info as GetAllAttachmentsRequestData, sessionID),
                typeof(GetAllAttachmentsRequestData)
            ),
            new RouteAction(
                "/client/mail/msg/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.SendMessage(url, info as SendMessageRequest, sessionID),
                typeof(SendMessageRequest)
            ),
            new RouteAction(
                "client/mail/dialog/clear",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.ClearMail(url, info as ClearMailMessageRequest, sessionID),
                typeof(ClearMailMessageRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/create",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.CreateGroupMail(url, info as CreateGroupMailRequest, sessionID),
                typeof(CreateGroupMailRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/owner/change",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.ChangeMailGroupOwner(url, info as ChangeGroupMailOwnerRequest, sessionID),
                typeof(ChangeGroupMailOwnerRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/users/add",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.AddUserToMail(url, info as AddUserGroupMailRequest, sessionID),
                typeof(AddUserGroupMailRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/users/remove",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.RemoveUserFromMail(url, info as RemoveUserGroupMailRequest, sessionID),
                typeof(RemoveUserGroupMailRequest)
            ),
            new RouteAction(
                "/client/friend/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.GetFriendList(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/list/outbox",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.ListOutbox(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/list/inbox",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.ListInbox(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.SendFriendRequest(url, info as FriendRequestData, sessionID),
                typeof(FriendRequestData)
            ),
            new RouteAction(
                "/client/friend/request/accept-all",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.AcceptAllFriendRequests(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/accept",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.AcceptFriendRequest(url, info as AcceptFriendRequestData, sessionID),
                typeof(AcceptFriendRequestData)
            ),
            new RouteAction(
                "/client/friend/request/decline",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.DeclineFriendRequest(url, info as DeclineFriendRequestData, sessionID),
                typeof(DeclineFriendRequestData)
            ),
            new RouteAction(
                "/client/friend/request/cancel",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.CancelFriendRequest(url, info as CancelFriendRequestData, sessionID),
                typeof(CancelFriendRequestData)
            ),
            new RouteAction(
                "/client/friend/delete",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.DeleteFriend(url, info as DeleteFriendRequest, sessionID),
                typeof(DeleteFriendRequest)
            ),
            new RouteAction(
                "/client/friend/ignore/set",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.IgnoreFriend(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            ),
            new RouteAction(
                "/client/friend/ignore/remove",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dialogueCallbacks.UnIgnoreFriend(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            )
        ]
    )
    {
        _dialogueCallbacks = dialogueCallbacks;
    }
}
