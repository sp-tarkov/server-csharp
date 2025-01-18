using Core.Annotations;
using Core.Helpers;
using Core.Helpers.Dialogue;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Controllers;

[Injectable]
public class DialogueController(
    ISptLogger<DialogueController> _logger,
    TimeUtil _timeUtil,
    DialogueHelper _dialogueHelper,
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
    /// 
    /// </summary>
    /// <param name="chatBot"></param>
    public void RegisterChatBot(IDialogueChatBot chatBot) // TODO: this is in with the helper types
    {
        if (_dialogueChatBots.Any(cb => cb.GetChatBot().Id == chatBot.GetChatBot().Id))
        {
            _logger.Error(_localisationService.GetText("dialog-chatbot_id_already_exists", chatBot.GetChatBot().Id));
        }

        _dialogueChatBots.Add(chatBot);
    }


    /// <summary>
    /// Handle onUpdate spt event
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
    /// Handle client/friend/list
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <returns>GetFriendListDataResponse</returns>
    public GetFriendListDataResponse GetFriendList(string sessionId)
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
            InIgnoreList = []
        };
    }

    private List<UserDialogInfo> GetActiveChatBots()
    {
        var activeBots = new List<UserDialogInfo>();

        var chatBotConfig = _coreConfig.Features.ChatbotFeatures;

        foreach (var bot in _dialogueChatBots)
        {
            var botData = bot.GetChatBot();
            if (chatBotConfig.EnabledBots.ContainsKey(botData.Id))
            {
                activeBots.Add(botData);
            }
        }

        return activeBots;
    }

    /// <summary>
    /// Handle client/mail/dialog/list
    ///	Create array holding trader dialogs and mail interactions with player
    /// Set the content of the dialogue on the list tab.
    /// </summary>
    /// <param name="sessionId">Session Id</param>
    /// <returns>list of dialogs</returns>
    public List<DialogueInfo> GenerateDialogueList(string sessionId)
    {
        var data = new List<DialogueInfo>();
        foreach (var dialogueId in _dialogueHelper.GetDialogsForProfile(sessionId))
        {
            data.Add(GetDialogueInfo(dialogueId.Key, sessionId));
        }

        return data;
    }

    /// <summary>
    /// Get the content of a dialogue
    /// </summary>
    /// <param name="dialogueId">Dialog id</param>
    /// <param name="sessionId">Session Id</param>
    /// <returns>DialogueInfo</returns>
    public DialogueInfo GetDialogueInfo(
        string? dialogueId,
        string sessionId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        var dialogue = dialogs.GetValueOrDefault(dialogueId);

        var result = new DialogueInfo
        {
            Id = dialogueId,
            Type = dialogue.Type ?? MessageType.NPC_TRADER,
            Message = _dialogueHelper.GetMessagePreview(dialogue),
            New = dialogue.New,
            AttachmentsNew = dialogue.AttachmentsNew,
            Pinned = dialogue.Pinned,
            Users = GetDialogueUsers(dialogue, dialogue.Type.Value, sessionId),
        };

        return result;
    }

    /// <summary>
    /// Get the users involved in a dialog (player + other party)
    /// </summary>
    /// <param name="dialog">The dialog to check for users</param>
    /// <param name="messageType">What type of message is being sent</param>
    /// <param name="sessionId">Player id</param>
    /// <returns>UserDialogInfo list</returns>
    public List<UserDialogInfo> GetDialogueUsers(
        Dialogue dialog,
        MessageType messageType,
        string sessionId)
    {
        var profile = _saveServer.GetProfile(sessionId);

        // User to user messages are special in that they need the player to exist in them, add if they don't
        if (messageType == MessageType.USER_MESSAGE &&
            dialog.Users.All(userDialog => userDialog.Id != profile.CharacterData.PmcData.SessionId))
        {
            // Nullguard
            dialog.Users ??= [];

            dialog.Users.Add(
                new UserDialogInfo
                {
                    Id = profile.CharacterData.PmcData.SessionId,
                    Aid = profile.CharacterData.PmcData.Aid,
                    Info = new UserDialogDetails
                    {
                        Level = profile.CharacterData.PmcData.Info.Level,
                        Nickname = profile.CharacterData.PmcData.Info.Nickname,
                        Side = profile.CharacterData.PmcData.Info.Side,
                        MemberCategory = profile.CharacterData.PmcData.Info.MemberCategory,
                        SelectedMemberCategory = profile.CharacterData.PmcData.Info.SelectedMemberCategory,
                    },
                }
            );
        }

        return dialog.Users;
    }

    /// <summary>
    /// Handle client/mail/dialog/view
    /// Handle player clicking 'messenger' and seeing all the messages they've received
    /// Set the content of the dialogue on the details panel, showing all the messages
    /// for the specified dialogue.
    /// </summary>
    /// <param name="request">Get dialog request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>GetMailDialogViewResponseData object</returns>
    public GetMailDialogViewResponseData GenerateDialogueView(
        GetMailDialogViewRequestData request,
        string sessionId)
    {
        var dialogueId = request.DialogId;
        var fullProfile = _saveServer.GetProfile(sessionId);
        var dialogue = GetDialogByIdFromProfile(fullProfile, request);

        // Dialog was opened, remove the little [1] on screen
        dialogue.New = 0;

        // Set number of new attachments, but ignore those that have expired.
        dialogue.AttachmentsNew = GetUnreadMessagesWithAttachmentsCount(sessionId, dialogueId);

        return new GetMailDialogViewResponseData
        {
            Messages = dialogue.Messages,
            Profiles = GetProfilesForMail(fullProfile, dialogue.Users),
            HasMessagesWithRewards = MessagesHaveUncollectedRewards(dialogue.Messages),
        };
    }

    /// <summary>
    /// Get dialog from player profile, create if doesn't exist
    /// </summary>
    /// <param name="profile">Player profile</param>
    /// <param name="request">get dialog request</param>
    /// <returns>Dialogue</returns>
    private Dialogue GetDialogByIdFromProfile(
        SptProfile profile,
        GetMailDialogViewRequestData request)
    {
        if (!profile.DialogueRecords.ContainsKey(request.DialogId))
        {
            profile.DialogueRecords[request.DialogId] = new Dialogue
            {
                Id = request.DialogId,
                AttachmentsNew = 0,
                Pinned = false,
                Messages = [],
                New = 0,
                Type = request.Type,
            };

            if (request.Type == MessageType.USER_MESSAGE)
            {
                var dialogue = profile.DialogueRecords[request.DialogId];
                dialogue.Users = [];
                var chatBot = _dialogueChatBots.FirstOrDefault((cb) => cb.GetChatBot().Id == request.DialogId);
                if (chatBot is not null)
                {
                    dialogue.Users ??= [];
                    dialogue.Users.Add(chatBot.GetChatBot());
                }
            }
        }

        return profile.DialogueRecords[request.DialogId];
    }

    /// <summary>
    /// Get the users involved in a mail between two entities
    /// </summary>
    /// <param name="fullProfile">Player profile</param>
    /// <param name="userDialogs">The participants of the mail</param>
    /// <returns>UserDialogInfo list</returns>
    private List<UserDialogInfo> GetProfilesForMail(SptProfile fullProfile, List<UserDialogInfo>? userDialogs)
    {
        List<UserDialogInfo> result = [];
        if (userDialogs is null)
        {
            // Nothing to add
            return result;
        }

        result.AddRange(userDialogs);

        if (result.All(userDialog => userDialog.Id != fullProfile.ProfileInfo.ProfileId))
        {
            // Player doesn't exist, add them in before returning
            var pmcProfile = fullProfile.CharacterData.PmcData;
            result.Add(
                new UserDialogInfo
                {
                    Id = fullProfile.ProfileInfo.ProfileId,
                    Aid = fullProfile.ProfileInfo.Aid,
                    Info = new UserDialogDetails
                    {
                        Nickname = pmcProfile.Info.Nickname,
                        Side = pmcProfile.Info.Side,
                        Level = pmcProfile.Info.Level,
                        MemberCategory = pmcProfile.Info.MemberCategory,
                        SelectedMemberCategory = pmcProfile.Info.SelectedMemberCategory,
                    }
                }
            );
        }

        return result;
    }

    /// <summary>
    /// Get a count of messages with attachments from a particular dialog
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="dialogueId">Dialog id</param>
    /// <returns>Count of messages with attachments</returns>
    private int GetUnreadMessagesWithAttachmentsCount(
        string sessionId,
        string dialogueId)
    {
        var newAttachmentCount = 0;
        var activeMessages = GetActiveMessagesFromDialog(sessionId, dialogueId);
        foreach (var message in activeMessages)
        {
            if (message.HasRewards.GetValueOrDefault(false) && !message.RewardCollected.GetValueOrDefault(false))
            {
                newAttachmentCount++;
            }
        }

        return newAttachmentCount;
    }

    /**
     * Get messages from a specific dialog that have items not expired
     * @param sessionId Session id
     * @param dialogueId Dialog to get mail attachments from
     * @returns Message array
     */
    protected List<Message> GetActiveMessagesFromDialog(string sessionId, string dialogueId)
    {
        var timeNow = _timeUtil.GetTimeStamp();
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);

        return dialogs[dialogueId].Messages.Where((message) => timeNow < message.DateTime + (message.MaxStorageTime ?? 0)).ToList();
    }

    /// <summary>
    /// Does list have messages with uncollected rewards (includes expired rewards)
    /// </summary>
    /// <param name="messages">Messages to check</param>
    /// <returns>true if uncollected rewards found</returns>
    private bool MessagesHaveUncollectedRewards(List<Message> messages)
    {
        return messages.Any((message) => (message.Items?.Data?.Count ?? 0) > 0);
    }

    /// <summary>
    /// Handle client/mail/dialog/remove
    /// Remove an entire dialog with an entity (trader/user)
    /// </summary>
    /// <param name="dialogueId">id of the dialog to remove</param>
    /// <param name="sessionId">Player id</param>
    public void RemoveDialogue(
        string? dialogueId,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/pin && Handle client/mail/dialog/unpin
    /// </summary>
    /// <param name="dialogueId"></param>
    /// <param name="shouldPin"></param>
    /// <param name="sessionId"></param>
    public void SetDialoguePin(
        string? dialogueId,
        bool shouldPin,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/read
    /// Set a dialog to be read (no number alert/attachment alert)
    /// </summary>
    /// <param name="dialogueIds">Dialog ids to set as read</param>
    /// <param name="sessionId">Player profile id</param>
    public void SetRead(
        List<string>? dialogueIds,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/getAllAttachments
    /// Get all uncollected items attached to mail in a particular dialog
    /// </summary>
    /// <param name="dialogueId">Dialog to get mail attachments from</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>GetAllAttachmentsResponse or null if dialogue doesnt exist</returns>
    public GetAllAttachmentsResponse? GetAllAttachments(
        string dialogueId,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// handle client/mail/msg/send
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public string SendMessage(
        string sessionId,
        SendMessageRequest request)
    {
        _mailSendService.SendPlayerMessageToNpc(sessionId, request.DialogId, request.Text);

        return (
            _dialogueChatBots
                .FirstOrDefault((cb) => cb.GetChatBot().Id == request.DialogId)
                ?.HandleMessage(sessionId, request) ??
            request.DialogId
        );
    }

    /// <summary>
    /// Get messages from a specific dialog that have items not expired
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="dialogueId">Dialog to get mail attachments from</param>
    /// <returns>message list</returns>
    private List<Message> GetActiveMessagesFromDialogue(
        string sessionId,
        string dialogueId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return list of messages with uncollected items (includes expired)
    /// </summary>
    /// <param name="messages">Messages to parse</param>
    /// <returns>messages with items to collect</returns>
    private List<Message> GetMessageWithAttachments(List<Message> messages)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete expired items from all messages in player profile. triggers when updating traders.
    /// </summary>
    /// <param name="sessionId">Session id</param>
    private void RemoveExpiredItemsFromMessages(string sessionId)
    {
        foreach (var dialogueId in _dialogueHelper.GetDialogsForProfile(sessionId))
        {
            RemoveExpiredItemsFromMessage(sessionId, dialogueId.Key);
        }
    }

    /// <summary>
    /// Removes expired items from a message in player profile
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="dialogueId">Dialog id</param>
    private void RemoveExpiredItemsFromMessage(
        string sessionId,
        string dialogueId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        if (!dialogs.TryGetValue(dialogueId, out var dialog))
        {
            return;
        }

        foreach (var message in dialog.Messages)
        {
            if (MessageHasExpired(message))
            {
                message.Items = new MessageItems();
            }
        }
    }

    /**
     * Has a dialog message expired
     * @param message Message to check expiry of
     * @returns true or false
     */
    protected bool MessageHasExpired(Message message)
    {
        return _timeUtil.GetTimeStamp() > message.DateTime + (message.MaxStorageTime ?? 0);
    }

    public FriendRequestSendResponse SendFriendRequest(string sessionId, FriendRequestData request)
    {
        throw new NotImplementedException();
    }

    public void DeleteFriend(string sessionID, DeleteFriendRequest request)
    {
        throw new NotImplementedException();
    }
}
