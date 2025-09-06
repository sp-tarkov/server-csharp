using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class DialogStaticRouter(JsonUtil jsonUtil, DialogueCallbacks dialogueCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<GetChatServerListRequestData>(
                "/client/chatServer/list",
                async (url, info, sessionID, output) => await dialogueCallbacks.GetChatServerList(url, info, sessionID)
            ),
            new RouteAction<GetMailDialogListRequestData>(
                "/client/mail/dialog/list",
                async (url, info, sessionID, output) => await dialogueCallbacks.GetMailDialogList(url, info, sessionID)
            ),
            new RouteAction<GetMailDialogViewRequestData>(
                "/client/mail/dialog/view",
                async (url, info, sessionID, output) => await dialogueCallbacks.GetMailDialogView(url, info, sessionID)
            ),
            new RouteAction<GetMailDialogInfoRequestData>(
                "/client/mail/dialog/info",
                async (url, info, sessionID, output) => await dialogueCallbacks.GetMailDialogInfo(url, info, sessionID)
            ),
            new RouteAction<RemoveDialogRequestData>(
                "/client/mail/dialog/remove",
                async (url, info, sessionID, output) => await dialogueCallbacks.RemoveDialog(url, info, sessionID)
            ),
            new RouteAction<PinDialogRequestData>(
                "/client/mail/dialog/pin",
                async (url, info, sessionID, output) => await dialogueCallbacks.PinDialog(url, info, sessionID)
            ),
            new RouteAction<PinDialogRequestData>(
                "/client/mail/dialog/unpin",
                async (url, info, sessionID, output) => await dialogueCallbacks.UnpinDialog(url, info, sessionID)
            ),
            new RouteAction<SetDialogReadRequestData>(
                "/client/mail/dialog/read",
                async (url, info, sessionID, output) => await dialogueCallbacks.SetRead(url, info, sessionID)
            ),
            new RouteAction<GetAllAttachmentsRequestData>(
                "/client/mail/dialog/getAllAttachments",
                async (url, info, sessionID, output) => await dialogueCallbacks.GetAllAttachments(url, info, sessionID)
            ),
            new RouteAction<SendMessageRequest>(
                "/client/mail/msg/send",
                async (url, info, sessionID, output) => await dialogueCallbacks.SendMessage(url, info, sessionID)
            ),
            new RouteAction<ClearMailMessageRequest>(
                "/client/mail/dialog/clear",
                async (url, info, sessionID, output) => await dialogueCallbacks.ClearMail(url, info, sessionID)
            ),
            new RouteAction<CreateGroupMailRequest>(
                "/client/mail/dialog/group/create",
                async (url, info, sessionID, output) => await dialogueCallbacks.CreateGroupMail(url, info, sessionID)
            ),
            new RouteAction<ChangeGroupMailOwnerRequest>(
                "/client/mail/dialog/group/owner/change",
                async (url, info, sessionID, output) => await dialogueCallbacks.ChangeMailGroupOwner(url, info, sessionID)
            ),
            new RouteAction<AddUserGroupMailRequest>(
                "/client/mail/dialog/group/users/add",
                async (url, info, sessionID, output) => await dialogueCallbacks.AddUserToMail(url, info, sessionID)
            ),
            new RouteAction<RemoveUserGroupMailRequest>(
                "/client/mail/dialog/group/users/remove",
                async (url, info, sessionID, output) => await dialogueCallbacks.RemoveUserFromMail(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/friend/list",
                async (url, info, sessionID, output) => await dialogueCallbacks.GetFriendList(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/friend/request/list/outbox",
                async (url, info, sessionID, output) => await dialogueCallbacks.ListOutbox(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/friend/request/list/inbox",
                async (url, info, sessionID, output) => await dialogueCallbacks.ListInbox(url, info, sessionID)
            ),
            new RouteAction<FriendRequestData>(
                "/client/friend/request/send",
                async (url, info, sessionID, output) => await dialogueCallbacks.SendFriendRequest(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/friend/request/accept-all",
                async (url, info, sessionID, output) => await dialogueCallbacks.AcceptAllFriendRequests(url, info, sessionID)
            ),
            new RouteAction<AcceptFriendRequestData>(
                "/client/friend/request/accept",
                async (url, info, sessionID, output) => await dialogueCallbacks.AcceptFriendRequest(url, info, sessionID)
            ),
            new RouteAction<DeclineFriendRequestData>(
                "/client/friend/request/decline",
                async (url, info, sessionID, output) => await dialogueCallbacks.DeclineFriendRequest(url, info, sessionID)
            ),
            new RouteAction<CancelFriendRequestData>(
                "/client/friend/request/cancel",
                async (url, info, sessionID, output) => await dialogueCallbacks.CancelFriendRequest(url, info, sessionID)
            ),
            new RouteAction<DeleteFriendRequest>(
                "/client/friend/delete",
                async (url, info, sessionID, output) => await dialogueCallbacks.DeleteFriend(url, info, sessionID)
            ),
            new RouteAction<UIDRequestData>(
                "/client/friend/ignore/set",
                async (url, info, sessionID, output) => await dialogueCallbacks.IgnoreFriend(url, info, sessionID)
            ),
            new RouteAction<UIDRequestData>(
                "/client/friend/ignore/remove",
                async (url, info, sessionID, output) => await dialogueCallbacks.UnIgnoreFriend(url, info, sessionID)
            ),
        ]
    ) { }
