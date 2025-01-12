using Core.Annotations;
using Core.Helpers;
using Core.Helpers.Dialogue;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Servers;

namespace Core.Controllers;

[Injectable]
public class DialogueController
{
    private readonly DialogueHelper _dialogueHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly SaveServer _saveServer;
    private readonly List<IDialogueChatBot> _dialogueChatBots;

    public DialogueController(
        DialogueHelper dialogueHelper,
        ProfileHelper profileHelper,
        SaveServer saveServer)
    {
        _dialogueHelper = dialogueHelper;
        _profileHelper = profileHelper;
        _saveServer = saveServer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chatBot"></param>
    /*
    public void RegisterChatBot(DialogueChatBot chatBot) // TODO: this is in with the helper types
    {
        throw new NotImplementedException();
    }
    */

    /// <summary>
    /// Handle onUpdate spt event
    /// </summary>
    public void Update()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/friend/list
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <returns>GetFriendListDataResponse</returns>
    public GetFriendListDataResponse GetFriendList(string sessionId)
    {
        // Add all chatbots to the friends list
        var friends = _dialogueChatBots.Select((bot) => bot.GetChatBot()).ToList();

        // Add any friends the user has after the chatbots
        var profile = _profileHelper.GetFullProfile(sessionId);
        if (profile?.FriendProfileIds is not null)
        {
            foreach (var friendId in profile.FriendProfileIds) {
                var friendProfile = _profileHelper.GetChatRoomMemberFromSessionId(friendId);
                if (friendProfile is not null)
                {
                    friends.Add(new UserDialogInfo
                    {
                        Id = friendProfile.Id,
                        Aid = friendProfile.Aid,
                        Info = friendProfile.Info,
                    } );
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
        string dialogueId,
        string sessionId)
    {
        var dialogs = _dialogueHelper.GetDialogsForProfile(sessionId);
        var dialogue = dialogs.GetValueOrDefault(dialogueId);

        var result = new DialogueInfo {
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
    /// <param name="dialogue">The dialog to check for users</param>
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
        if (
            messageType == MessageType.USER_MESSAGE &&
            !dialog.Users.Any((userDialog) => userDialog.Id == profile.CharacterData.PmcData.SessionId))
        {
            // nullguard
            dialog.Users ??= [];

            dialog.Users.Add( new UserDialogInfo
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
            });
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the users involved in a mail between two entities
    /// </summary>
    /// <param name="fullProfile">Player profile</param>
    /// <param name="userDialogs">The participants of the mail</param>
    /// <returns>UserDialogInfo list</returns>
    private List<UserDialogInfo> GetProfilesForMail(
        SptProfile fullProfile,
        List<UserDialogInfo>? userDialogs)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does list have messages with uncollected rewards (includes expired rewards)
    /// </summary>
    /// <param name="messages">Messages to check</param>
    /// <returns>true if uncollected rewards found</returns>
    private bool MessagesHaveUncollectedRewards(List<Message> messages)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/mail/dialog/remove
    /// Remove an entire dialog with an entity (trader/user)
    /// </summary>
    /// <param name="dialogueId">id of the dialog to remove</param>
    /// <param name="sessionId">Player id</param>
    public void RemoveDialogue(
        string dialogueId,
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
        string dialogueId,
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
        List<string> dialogueIds,
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
