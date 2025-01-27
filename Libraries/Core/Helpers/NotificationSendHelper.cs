using SptCommon.Annotations;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ws;
using Core.Models.Enums;
using Core.Servers;
using Core.Servers.Ws;
using Core.Services;
using Core.Utils;

namespace Core.Helpers;

[Injectable]
public class NotificationSendHelper(
    IWebSocketConnectionHandler _sptWebSocketConnectionHandler,
    HashUtil _hashUtil,
    SaveServer _saveServer,
    NotificationService _notificationService,
    TimeUtil _timeUtil
)
{
    /// <summary>
    /// Send notification message to the appropriate channel
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="notificationMessage"></param>
    public void SendMessage(string sessionID, WsNotificationEvent notificationMessage)
    {
        if (_sptWebSocketConnectionHandler.IsWebSocketConnected(sessionID))
        {
            _sptWebSocketConnectionHandler.SendMessage(sessionID, notificationMessage);
        }
        else
        {
            _notificationService.Add(sessionID, notificationMessage);
        }
    }

    /// <summary>
    /// Send a message directly to the player
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="senderDetails">Who is sending the message to player</param>
    /// <param name="messageText">Text to send player</param>
    /// <param name="messageType">Underlying type of message being sent</param>
    public void SendMessageToPlayer(
        string sessionId,
        UserDialogInfo senderDetails,
        string messageText,
        MessageType messageType)
    {
        var dialog = GetDialog(sessionId, messageType, senderDetails);

        dialog.New += 1;
        Message message = new Message {
            Id = _hashUtil.Generate(),
            UserId = dialog.Id,
            MessageType = messageType,
            DateTime = _timeUtil.GetTimeStamp(),
            Text = messageText,
            HasRewards = null,
            RewardCollected = null,
            Items = null,
        };
        dialog.Messages.Add(message);

        WsChatMessageReceived notification = new WsChatMessageReceived {
            EventType = NotificationEventType.new_message,
            EventIdentifier = message.Id,
            DialogId = message.UserId,
            Message = message,
        };
        SendMessage(sessionId, notification);
    }

    /// <summary>
    /// Helper function for SendMessageToPlayer(), get new dialog for storage in profile or find existing by sender id
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="messageType">Type of message to generate</param>
    /// <param name="senderDetails">Who is sending the message</param>
    /// <returns>Dialogue</returns>
    protected Models.Eft.Profile.Dialogue GetDialog(string sessionId, MessageType messageType, UserDialogInfo senderDetails)
    {
        // Use trader id if sender is trader, otherwise use nickname
        var key = senderDetails.Id;
        var dialogueData = _saveServer.GetProfile(sessionId).DialogueRecords;
        var isNewDialogue = dialogueData.ContainsKey(key);
        var dialogue = dialogueData[key];

        // Existing dialog not found, make new one
        if (isNewDialogue) {
            dialogue = new Models.Eft.Profile.Dialogue {
                Id = key,
                Type = messageType,
                Messages = [],
                Pinned = false,
                New = 0,
                AttachmentsNew = 0,
                Users = senderDetails.Info.MemberCategory == MemberCategory.Trader ? null : [senderDetails],
            };

            dialogueData[key] = dialogue;
        }
        return dialogue;
    }
}
