using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Dialogue;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class DialogueController(
    ISptLogger<DialogueController> _logger,
    TimeUtil _timeUtil,
    DialogueHelper _dialogueHelper,
    NotificationSendHelper _notificationSendHelper,
    ProfileHelper _profileHelper,
    ConfigServer _configServer,
    SaveServer _saveServer,
    LocalisationService _localisationService,
    MailSendService _mailSendService,
    IEnumerable<IDialogueChatBot> dialogueChatBots
)
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected List<IDialogueChatBot> _dialogueChatBots = dialogueChatBots.ToList();

    /// <summary>
    /// </summary>
    /// <param name="chatBot"></param>
    public virtual void RegisterChatBot(IDialogueChatBot chatBot) // TODO: this is in with the helper types
    {
        if (_dialogueChatBots.Any(cb => cb.GetChatBot().Id == chatBot.GetChatBot().Id))
        {
            _logger.Error(
                _localisationService.GetText(
                    "dialog-chatbot_id_already_exists",
                    chatBot.GetChatBot().Id
                )
            );
        }

        _dialogueChatBots.Add(chatBot);
    }

    /// <summary>
    ///     Handle onUpdate spt event
    /// </summary>
    public void Update()
    {
        var profiles = _saveServer.GetProfiles();
        foreach (var kvp in profiles)
        {
            RemoveExpiredItemsFromMessages(kvp.Key);
        }
    }

    /// <summary>
    ///     Handle client/friend/list
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <returns>GetFriendListDataResponse</returns>
    public virtual GetFriendListDataResponse GetFriendList(string sessionId)
    {
        // Add all chatbots to the friends list
        var friends = GetActiveChatBots();

        // Add any friends the user has after the chatbots
        var profile = _profileHelper.GetFullProfile(sessionId);
        if (profile?.FriendProfileIds is not null)
        {
            foreach (var friendId in profile.FriendProfileIds)
            {
                var friendProfile = _profileHelper.GetChatRoomMemberFromSessionId(friendId);
                if (friendProfile is not null)
                {
                    friends.Add(
                        new UserDialogInfo
                        {
                            Id = friendProfile.Id,
                            Aid = friendProfile.Aid,
                            Info = friendProfile.Info,
                        }
                    );
                }
            }
        }

        return new GetFriendListDataResponse
        {
            Friends = friends,
            Ignore = [],
            InIgnoreList = [],
        };
    }

    protected List<UserDialogInfo> GetActiveChatBots()
    {
        var activeBots = new List<UserDialogInfo>();

        var chatBotConfig = _coreConfig.Features.ChatbotFeatures;

        foreach (var bot in _dialogueChatBots)
        {
            var botData = bot.GetChatBot();
            if (chatBotConfig.EnabledBots.ContainsKey(botData.Id!))
            {
                activeBots.Add(botData);
            }
        }

        return activeBots;
    }

    /// <summary>
    ///     Handle client/mail/dialog/list
    ///     Create array holding trader dialogs and mail interactions with player
    ///     Set the content of the dialogue on the list tab.
    /// </summary>
    /// <param name="sessionId">Session Id</param>
    /// <returns>list of dialogs</returns>
    public virtual List<DialogueInfo> GenerateDialogueList(string sessionId)
    {
        var data = new List<DialogueInfo>();
        foreach (var dialogueId in _dialogueHelper.GetDialogsForProfile(sessionId))
        {
            data.Add(GetDialogueInfo(dialogueId.Key, sessionId));
        }

        return data;
    }

    /// <summary>
    ///     Get the content of a dialogue
    /// </summary>
    /// <param name="dialogueId">Dialog id</param>
    /// <param name="sessionId">Session Id</param>
    /// <returns>DialogueInfo</returns>
    public virtual DialogueInfo GetDialogueInfo(string? dialogueId, string sessionId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        var dialogue = dialogs!.GetValueOrDefault(dialogueId);

        var result = new DialogueInfo
        {
            Id = dialogueId,
            Type = dialogue?.Type ?? MessageType.NPC_TRADER,
            Message = _dialogueHelper.GetMessagePreview(dialogue),
            New = dialogue?.New,
            AttachmentsNew = dialogue?.AttachmentsNew,
            Pinned = dialogue?.Pinned,
            Users = GetDialogueUsers(dialogue, dialogue?.Type, sessionId),
        };

        return result;
    }

    /// <summary>
    ///     Get the users involved in a dialog (player + other party)
    /// </summary>
    /// <param name="dialog">The dialog to check for users</param>
    /// <param name="messageType">What type of message is being sent</param>
    /// <param name="sessionId">Player id</param>
    /// <returns>UserDialogInfo list</returns>
    public virtual List<UserDialogInfo> GetDialogueUsers(
        Dialogue? dialog,
        MessageType? messageType,
        string sessionId
    )
    {
        var profile = _saveServer.GetProfile(sessionId);

        // User to user messages are special in that they need the player to exist in them, add if they don't
        if (
            messageType == MessageType.USER_MESSAGE
            && dialog?.Users is not null
            && dialog.Users.All(userDialog =>
                userDialog.Id != profile.CharacterData?.PmcData?.SessionId
            )
        )
        {
            dialog.Users.Add(
                new UserDialogInfo
                {
                    Id = profile.CharacterData?.PmcData?.SessionId,
                    Aid = profile.CharacterData?.PmcData?.Aid,
                    Info = new UserDialogDetails
                    {
                        Level = profile.CharacterData?.PmcData?.Info?.Level,
                        Nickname = profile.CharacterData?.PmcData?.Info?.Nickname,
                        Side = profile.CharacterData?.PmcData?.Info?.Side,
                        MemberCategory = profile.CharacterData?.PmcData?.Info?.MemberCategory,
                        SelectedMemberCategory = profile
                            .CharacterData
                            ?.PmcData
                            ?.Info
                            ?.SelectedMemberCategory,
                    },
                }
            );
        }

        return dialog?.Users!;
    }

    /// <summary>
    ///     Handle client/mail/dialog/view
    ///     Handle player clicking 'messenger' and seeing all the messages they've received
    ///     Set the content of the dialogue on the details panel, showing all the messages
    ///     for the specified dialogue.
    /// </summary>
    /// <param name="request">Get dialog request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>GetMailDialogViewResponseData object</returns>
    public virtual GetMailDialogViewResponseData GenerateDialogueView(
        GetMailDialogViewRequestData request,
        string sessionId
    )
    {
        var dialogueId = request.DialogId;
        var fullProfile = _saveServer.GetProfile(sessionId);
        var dialogue = GetDialogByIdFromProfile(fullProfile, request);

        // Dialog was opened, remove the little [1] on screen
        dialogue.New = 0;

        // Set number of new attachments, but ignore those that have expired.
        dialogue.AttachmentsNew = GetUnreadMessagesWithAttachmentsCount(sessionId, dialogueId!);

        return new GetMailDialogViewResponseData
        {
            Messages = dialogue.Messages,
            Profiles = GetProfilesForMail(fullProfile, dialogue.Users),
            HasMessagesWithRewards = MessagesHaveUncollectedRewards(dialogue.Messages!),
        };
    }

    /// <summary>
    ///     Get dialog from player profile, create if doesn't exist
    /// </summary>
    /// <param name="profile">Player profile</param>
    /// <param name="request">get dialog request</param>
    /// <returns>Dialogue</returns>
    protected Dialogue GetDialogByIdFromProfile(
        SptProfile profile,
        GetMailDialogViewRequestData request
    )
    {
        if (
            profile.DialogueRecords is null
            || profile.DialogueRecords.ContainsKey(request.DialogId!)
        )
        {
            return profile.DialogueRecords?[request.DialogId!]
                ?? throw new NullReferenceException();
        }

        profile.DialogueRecords[request.DialogId!] = new Dialogue
        {
            Id = request.DialogId,
            AttachmentsNew = 0,
            Pinned = false,
            Messages = [],
            New = 0,
            Type = request.Type,
        };

        if (request.Type != MessageType.USER_MESSAGE)
        {
            return profile.DialogueRecords[request.DialogId!];
        }

        var dialogue = profile.DialogueRecords[request.DialogId!];
        dialogue.Users = [];
        var chatBot = _dialogueChatBots.FirstOrDefault(cb =>
            cb.GetChatBot().Id == request.DialogId
        );

        if (chatBot is null)
        {
            return profile.DialogueRecords[request.DialogId!];
        }

        dialogue.Users ??= [];
        dialogue.Users.Add(chatBot.GetChatBot());

        return profile.DialogueRecords[request.DialogId!];
    }

    /// <summary>
    ///     Get the users involved in a mail between two entities
    /// </summary>
    /// <param name="fullProfile">Player profile</param>
    /// <param name="userDialogs">The participants of the mail</param>
    /// <returns>UserDialogInfo list</returns>
    protected List<UserDialogInfo> GetProfilesForMail(
        SptProfile fullProfile,
        List<UserDialogInfo>? userDialogs
    )
    {
        List<UserDialogInfo> result = [];
        if (userDialogs is null)
        // Nothing to add
        {
            return result;
        }

        result.AddRange(userDialogs);

        if (result.Any(userDialog => userDialog.Id == fullProfile.ProfileInfo?.ProfileId))
        {
            return result;
        }

        // Player doesn't exist, add them in before returning
        var pmcProfile = fullProfile.CharacterData?.PmcData;
        result.Add(
            new UserDialogInfo
            {
                Id = fullProfile.ProfileInfo?.ProfileId,
                Aid = fullProfile.ProfileInfo?.Aid,
                Info = new UserDialogDetails
                {
                    Nickname = pmcProfile?.Info?.Nickname,
                    Side = pmcProfile?.Info?.Side,
                    Level = pmcProfile?.Info?.Level,
                    MemberCategory = pmcProfile?.Info?.MemberCategory,
                    SelectedMemberCategory = pmcProfile?.Info?.SelectedMemberCategory,
                },
            }
        );

        return result;
    }

    /// <summary>
    ///     Get a count of messages with attachments from a particular dialog
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="dialogueId">Dialog id</param>
    /// <returns>Count of messages with attachments</returns>
    protected int GetUnreadMessagesWithAttachmentsCount(string sessionId, string dialogueId)
    {
        var newAttachmentCount = 0;
        var activeMessages = GetActiveMessagesFromDialog(sessionId, dialogueId);
        foreach (var message in activeMessages)
        {
            if (
                message.HasRewards.GetValueOrDefault(false)
                && !message.RewardCollected.GetValueOrDefault(false)
            )
            {
                newAttachmentCount++;
            }
        }

        return newAttachmentCount;
    }

    /// <summary>
    ///     Get messages from a specific dialog that have items not expired
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="dialogueId">Dialog to get mail attachments from</param>
    /// <returns>Message array</returns>
    protected List<Message> GetActiveMessagesFromDialog(string sessionId, string dialogueId)
    {
        var timeNow = _timeUtil.GetTimeStamp();
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);

        return dialogs[dialogueId]
                .Messages?.Where(message =>
                {
                    var checkTime = message.DateTime + (message.MaxStorageTime ?? 0);
                    return timeNow < checkTime;
                })
                .ToList() ?? [];
    }

    /// <summary>
    ///     Does list have messages with uncollected rewards (includes expired rewards)
    /// </summary>
    /// <param name="messages">Messages to check</param>
    /// <returns>true if uncollected rewards found</returns>
    protected bool MessagesHaveUncollectedRewards(List<Message> messages)
    {
        return messages.Any(message => (message.Items?.Data?.Count ?? 0) > 0);
    }

    /// <summary>
    ///     Handle client/mail/dialog/remove
    ///     Remove an entire dialog with an entity (trader/user)
    /// </summary>
    /// <param name="dialogueId">id of the dialog to remove</param>
    /// <param name="sessionId">Player id</param>
    public virtual void RemoveDialogue(string? dialogueId, string sessionId)
    {
        var profile = _saveServer.GetProfile(sessionId);
        var dialog = profile.DialogueRecords.GetValueOrDefault(dialogueId);
        if (dialog is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "dialogue-unable_to_find_in_profile",
                    new { sessionId, dialogueId }
                )
            );

            return;
        }

        profile.DialogueRecords.Remove(dialogueId);
    }

    /// <summary>
    ///     Handle client/mail/dialog/pin && Handle client/mail/dialog/unpin
    /// </summary>
    /// <param name="dialogueId"></param>
    /// <param name="shouldPin"></param>
    /// <param name="sessionId">Session/Player id</param>
    public virtual void SetDialoguePin(string? dialogueId, bool shouldPin, string sessionId)
    {
        var dialog = _dialogueHelper.GetDialogsForProfile(sessionId).GetValueOrDefault(dialogueId);
        if (dialog is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "dialogue-unable_to_find_in_profile",
                    new { sessionId, dialogueId }
                )
            );

            return;
        }

        dialog.Pinned = shouldPin;
    }

    /// <summary>
    ///     Handle client/mail/dialog/read
    ///     Set a dialog to be read (no number alert/attachment alert)
    /// </summary>
    /// <param name="dialogueIds">Dialog ids to set as read</param>
    /// <param name="sessionId">Player profile id</param>
    public virtual void SetRead(List<string>? dialogueIds, string sessionId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        if (dialogs is null || !dialogs.Any())
        {
            _logger.Error(
                _localisationService.GetText(
                    "dialogue-unable_to_find_dialogs_in_profile",
                    new { sessionId }
                )
            );

            return;
        }

        foreach (var dialogId in dialogueIds)
        {
            dialogs[dialogId].New = 0;
            dialogs[dialogId].AttachmentsNew = 0;
        }
    }

    /// <summary>
    ///     Handle client/mail/dialog/getAllAttachments
    ///     Get all uncollected items attached to mail in a particular dialog
    /// </summary>
    /// <param name="dialogueId">Dialog to get mail attachments from</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>GetAllAttachmentsResponse or null if dialogue doesnt exist</returns>
    public virtual GetAllAttachmentsResponse GetAllAttachments(string dialogueId, string sessionId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        var dialog = dialogs.TryGetValue(dialogueId, out var dialogInfo);
        if (!dialog)
        {
            _logger.Error(_localisationService.GetText("dialogue-unable_to_find_in_profile"));

            return null;
        }

        // Removes corner 'new messages' tag
        dialogInfo.AttachmentsNew = 0;

        var activeMessages = GetActiveMessagesFromDialog(sessionId, dialogueId);
        var messagesWithAttachments = GetMessageWithAttachments(activeMessages);

        return new GetAllAttachmentsResponse
        {
            Messages = messagesWithAttachments,
            Profiles = [],
            HasMessagesWithRewards = MessagesHaveUncollectedRewards(messagesWithAttachments),
        };
    }

    /// <summary>
    ///     handle client/mail/msg/send
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual string SendMessage(string sessionId, SendMessageRequest request)
    {
        _mailSendService.SendPlayerMessageToNpc(sessionId, request.DialogId!, request.Text!);

        return (
                _dialogueChatBots
                    .FirstOrDefault(cb => cb.GetChatBot().Id == request.DialogId)
                    ?.HandleMessage(sessionId, request) ?? request.DialogId
            ) ?? string.Empty;
    }

    /// <summary>
    ///     Return list of messages with uncollected items (includes expired)
    /// </summary>
    /// <param name="messages">Messages to parse</param>
    /// <returns>messages with items to collect</returns>
    protected List<Message> GetMessageWithAttachments(List<Message> messages)
    {
        return messages.Where(message => (message.Items?.Data?.Count ?? 0) > 0).ToList();
    }

    /// <summary>
    ///     Delete expired items from all messages in player profile. triggers when updating traders.
    /// </summary>
    /// <param name="sessionId">Session id</param>
    protected void RemoveExpiredItemsFromMessages(string sessionId)
    {
        foreach (var dialogueId in _dialogueHelper.GetDialogsForProfile(sessionId))
        {
            RemoveExpiredItemsFromMessage(sessionId, dialogueId.Key);
        }
    }

    /// <summary>
    ///     Removes expired items from a message in player profile
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="dialogueId">Dialog id</param>
    protected void RemoveExpiredItemsFromMessage(string sessionId, string dialogueId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        if (!dialogs.TryGetValue(dialogueId, out var dialog))
        {
            return;
        }

        foreach (var message in dialog.Messages ?? [])
        {
            if (MessageHasExpired(message))
            {
                message.Items = new MessageItems();
            }
        }
    }

    /// <summary>
    ///     Has a dialog message expired
    /// </summary>
    /// <param name="message">Message to check expiry of</param>
    /// <returns>True = expired</returns>
    protected bool MessageHasExpired(Message message)
    {
        return _timeUtil.GetTimeStamp() > message.DateTime + (message.MaxStorageTime ?? 0);
    }

    /// <summary>
    ///     Handle client/friend/request/send
    /// </summary>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="request">Sent friend request</param>
    /// <returns></returns>
    public virtual FriendRequestSendResponse SendFriendRequest(
        string sessionID,
        FriendRequestData request
    )
    {
        // To avoid needing to jump between profiles, auto-accept all friend requests
        var friendProfile = _profileHelper.GetFullProfile(request.To);
        if (friendProfile?.CharacterData?.PmcData is null)
        {
            return new FriendRequestSendResponse
            {
                Status = BackendErrorCodes.PlayerProfileNotFound,
                RequestId = "", // Unused in an error state
                RetryAfter = 600,
            };
        }

        // Only add the profile to the friends list if it doesn't already exist
        var profile = _saveServer.GetProfile(sessionID);
        if (!profile.FriendProfileIds.Contains(request.To))
        {
            profile.FriendProfileIds.Add(request.To);
        }

        // We need to delay this so that the friend request gets properly added to the clientside list before we accept it
        _ = new Timer(
            _ =>
            {
                var notification = new WsFriendsListAccept
                {
                    EventType = NotificationEventType.friendListRequestAccept,
                    Profile = _profileHelper.GetChatRoomMemberFromPmcProfile(
                        friendProfile.CharacterData.PmcData
                    ),
                };
                _notificationSendHelper.SendMessage(sessionID, notification);
            },
            null,
            TimeSpan.FromMicroseconds(1000),
            Timeout.InfiniteTimeSpan // This should mean it does this callback once after 1 second and then stops
        );

        return new FriendRequestSendResponse
        {
            Status = BackendErrorCodes.None,
            RequestId = friendProfile.ProfileInfo.Aid.ToString(),
            RetryAfter = 600,
        };
    }

    /// <summary>
    ///     Handle client/friend/delete
    /// </summary>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="request">Sent delete friend request</param>
    public virtual void DeleteFriend(string sessionID, DeleteFriendRequest request)
    {
        var profile = _saveServer.GetProfile(sessionID);
        var friendIndex = profile.FriendProfileIds.IndexOf(request.FriendId);
        if (friendIndex != -1)
        {
            profile.FriendProfileIds.RemoveAt(friendIndex);
        }
    }
}
