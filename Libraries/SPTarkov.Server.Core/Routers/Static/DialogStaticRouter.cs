using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class DialogStaticRouter : StaticRouter
{
    public DialogStaticRouter(
        JsonUtil jsonUtil,
        DialogueCallbacks dialogueCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/chatServer/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.GetChatServerList(url, info as GetChatServerListRequestData, sessionID),
                typeof(GetChatServerListRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.GetMailDialogList(url, info as GetMailDialogListRequestData, sessionID),
                typeof(GetMailDialogListRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/view",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.GetMailDialogView(url, info as GetMailDialogViewRequestData, sessionID),
                typeof(GetMailDialogViewRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/info",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.GetMailDialogInfo(url, info as GetMailDialogInfoRequestData, sessionID),
                typeof(GetMailDialogInfoRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/remove",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.RemoveDialog(url, info as RemoveDialogRequestData, sessionID),
                typeof(RemoveDialogRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/pin",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.PinDialog(url, info as PinDialogRequestData, sessionID),
                typeof(PinDialogRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/unpin",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.UnpinDialog(url, info as PinDialogRequestData, sessionID),
                typeof(PinDialogRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/read",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.SetRead(url, info as SetDialogReadRequestData, sessionID),
                typeof(SetDialogReadRequestData)
            ),
            new RouteAction(
                "/client/mail/dialog/getAllAttachments",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.GetAllAttachments(url, info as GetAllAttachmentsRequestData, sessionID),
                typeof(GetAllAttachmentsRequestData)
            ),
            new RouteAction(
                "/client/mail/msg/send",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.SendMessage(url, info as SendMessageRequest, sessionID),
                typeof(SendMessageRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/clear",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.ClearMail(url, info as ClearMailMessageRequest, sessionID),
                typeof(ClearMailMessageRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/create",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.CreateGroupMail(url, info as CreateGroupMailRequest, sessionID),
                typeof(CreateGroupMailRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/owner/change",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.ChangeMailGroupOwner(url, info as ChangeGroupMailOwnerRequest, sessionID),
                typeof(ChangeGroupMailOwnerRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/users/add",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.AddUserToMail(url, info as AddUserGroupMailRequest, sessionID),
                typeof(AddUserGroupMailRequest)
            ),
            new RouteAction(
                "/client/mail/dialog/group/users/remove",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.RemoveUserFromMail(url, info as RemoveUserGroupMailRequest, sessionID),
                typeof(RemoveUserGroupMailRequest)
            ),
            new RouteAction(
                "/client/friend/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.GetFriendList(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/list/outbox",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.ListOutbox(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/list/inbox",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.ListInbox(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/send",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.SendFriendRequest(url, info as FriendRequestData, sessionID),
                typeof(FriendRequestData)
            ),
            new RouteAction(
                "/client/friend/request/accept-all",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.AcceptAllFriendRequests(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/friend/request/accept",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.AcceptFriendRequest(url, info as AcceptFriendRequestData, sessionID),
                typeof(AcceptFriendRequestData)
            ),
            new RouteAction(
                "/client/friend/request/decline",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.DeclineFriendRequest(url, info as DeclineFriendRequestData, sessionID),
                typeof(DeclineFriendRequestData)
            ),
            new RouteAction(
                "/client/friend/request/cancel",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.CancelFriendRequest(url, info as CancelFriendRequestData, sessionID),
                typeof(CancelFriendRequestData)
            ),
            new RouteAction(
                "/client/friend/delete",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.DeleteFriend(url, info as DeleteFriendRequest, sessionID),
                typeof(DeleteFriendRequest)
            ),
            new RouteAction(
                "/client/friend/ignore/set",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.IgnoreFriend(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            ),
            new RouteAction(
                "/client/friend/ignore/remove",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dialogueCallbacks.UnIgnoreFriend(url, info as UIDRequestData, sessionID),
                typeof(UIDRequestData)
            )
        ]
    )
    {
    }
}
