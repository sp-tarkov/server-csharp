using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class DialogStaticRouter : StaticRouter
{
    public DialogStaticRouter(JsonUtil jsonUtil, DialogueCallbacks dialogueCallbacks)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/chatServer/list",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.GetChatServerList(
                            url,
                            info as GetChatServerListRequestData,
                            sessionID
                        );
                    },
                    typeof(GetChatServerListRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/list",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.GetMailDialogList(
                            url,
                            info as GetMailDialogListRequestData,
                            sessionID
                        );
                    },
                    typeof(GetMailDialogListRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/view",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.GetMailDialogView(
                            url,
                            info as GetMailDialogViewRequestData,
                            sessionID
                        );
                    },
                    typeof(GetMailDialogViewRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/info",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.GetMailDialogInfo(
                            url,
                            info as GetMailDialogInfoRequestData,
                            sessionID
                        );
                    },
                    typeof(GetMailDialogInfoRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/remove",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.RemoveDialog(
                            url,
                            info as RemoveDialogRequestData,
                            sessionID
                        );
                    },
                    typeof(RemoveDialogRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/pin",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.PinDialog(
                            url,
                            info as PinDialogRequestData,
                            sessionID
                        );
                    },
                    typeof(PinDialogRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/unpin",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.UnpinDialog(
                            url,
                            info as PinDialogRequestData,
                            sessionID
                        );
                    },
                    typeof(PinDialogRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/read",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.SetRead(
                            url,
                            info as SetDialogReadRequestData,
                            sessionID
                        );
                    },
                    typeof(SetDialogReadRequestData)
                ),
                new RouteAction(
                    "/client/mail/dialog/getAllAttachments",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.GetAllAttachments(
                            url,
                            info as GetAllAttachmentsRequestData,
                            sessionID
                        );
                    },
                    typeof(GetAllAttachmentsRequestData)
                ),
                new RouteAction(
                    "/client/mail/msg/send",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.SendMessage(
                            url,
                            info as SendMessageRequest,
                            sessionID
                        );
                    },
                    typeof(SendMessageRequest)
                ),
                new RouteAction(
                    "client/mail/dialog/clear",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.ClearMail(
                            url,
                            info as ClearMailMessageRequest,
                            sessionID
                        );
                    },
                    typeof(ClearMailMessageRequest)
                ),
                new RouteAction(
                    "/client/mail/dialog/group/create",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.CreateGroupMail(
                            url,
                            info as CreateGroupMailRequest,
                            sessionID
                        );
                    },
                    typeof(CreateGroupMailRequest)
                ),
                new RouteAction(
                    "/client/mail/dialog/group/owner/change",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.ChangeMailGroupOwner(
                            url,
                            info as ChangeGroupMailOwnerRequest,
                            sessionID
                        );
                    },
                    typeof(ChangeGroupMailOwnerRequest)
                ),
                new RouteAction(
                    "/client/mail/dialog/group/users/add",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.AddUserToMail(
                            url,
                            info as AddUserGroupMailRequest,
                            sessionID
                        );
                    },
                    typeof(AddUserGroupMailRequest)
                ),
                new RouteAction(
                    "/client/mail/dialog/group/users/remove",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.RemoveUserFromMail(
                            url,
                            info as RemoveUserGroupMailRequest,
                            sessionID
                        );
                    },
                    typeof(RemoveUserGroupMailRequest)
                ),
                new RouteAction(
                    "/client/friend/list",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.GetFriendList(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
                new RouteAction(
                    "/client/friend/request/list/outbox",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.ListOutbox(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
                new RouteAction(
                    "/client/friend/request/list/inbox",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.ListInbox(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
                new RouteAction(
                    "/client/friend/request/send",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.SendFriendRequest(
                            url,
                            info as FriendRequestData,
                            sessionID
                        );
                    },
                    typeof(FriendRequestData)
                ),
                new RouteAction(
                    "/client/friend/request/accept-all",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.AcceptAllFriendRequests(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
                new RouteAction(
                    "/client/friend/request/accept",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.AcceptFriendRequest(
                            url,
                            info as AcceptFriendRequestData,
                            sessionID
                        );
                    },
                    typeof(AcceptFriendRequestData)
                ),
                new RouteAction(
                    "/client/friend/request/decline",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.DeclineFriendRequest(
                            url,
                            info as DeclineFriendRequestData,
                            sessionID
                        );
                    },
                    typeof(DeclineFriendRequestData)
                ),
                new RouteAction(
                    "/client/friend/request/cancel",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.CancelFriendRequest(
                            url,
                            info as CancelFriendRequestData,
                            sessionID
                        );
                    },
                    typeof(CancelFriendRequestData)
                ),
                new RouteAction(
                    "/client/friend/delete",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.DeleteFriend(
                            url,
                            info as DeleteFriendRequest,
                            sessionID
                        );
                    },
                    typeof(DeleteFriendRequest)
                ),
                new RouteAction(
                    "/client/friend/ignore/set",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.IgnoreFriend(
                            url,
                            info as UIDRequestData,
                            sessionID
                        );
                    },
                    typeof(UIDRequestData)
                ),
                new RouteAction(
                    "/client/friend/ignore/remove",
                    (url, info, sessionID, output) =>
                    {
                        return dialogueCallbacks.UnIgnoreFriend(
                            url,
                            info as UIDRequestData,
                            sessionID
                        );
                    },
                    typeof(UIDRequestData)
                ),
            ]
        ) { }
}
