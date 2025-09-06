using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class NotificationSendHelper(
    ISptLogger<NotificationSendHelper> logger,
    SptWebSocketConnectionHandler sptWebSocketConnectionHandler,
    SaveServer saveServer,
    NotificationService notificationService,
    TimeUtil timeUtil,
    JsonUtil jsonUtil
)
{
    /// <summary>
    ///     Send notification message to the appropriate channel
    /// </summary>
    /// <param name="sessionId">Session/player id</param>
    /// <param name="notificationMessage"></param>
    public void SendMessage(MongoId sessionId, WsNotificationEvent notificationMessage)
    {
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Send message for {sessionId} started, message: {jsonUtil.Serialize(notificationMessage)}");
        }

        if (sptWebSocketConnectionHandler.IsWebSocketConnected(sessionId))
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Send message for {sessionId} websocket available, message being sent");
            }
            sptWebSocketConnectionHandler.SendMessage(sessionId, notificationMessage);
            return;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Send message for {sessionId} websocket not available, queueing into profile");
        }

        notificationService.Add(sessionId, notificationMessage);
    }

    /// <summary>
    ///     Send a message directly to the player
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="senderDetails">Who is sending the message to player</param>
    /// <param name="messageText">Text to send player</param>
    /// <param name="messageType">Underlying type of message being sent</param>
    public void SendMessageToPlayer(MongoId sessionId, UserDialogInfo senderDetails, string messageText, MessageType messageType)
    {
        var dialog = GetDialog(sessionId, messageType, senderDetails);
        if (dialog is null)
        {
            // Error is logged in GetDialog
            return;
        }

        dialog.New += 1;
        var message = new Message
        {
            Id = new MongoId(),
            UserId = dialog.Id,
            MessageType = messageType,
            DateTime = timeUtil.GetTimeStamp(),
            Text = messageText,
            HasRewards = null,
            RewardCollected = null,
            Items = null,
        };

        if (dialog.Messages != null)
        {
            dialog.Messages.Add(message);
        }
        else
        {
            logger.Error(
                $"Could not add message Id: {message.Id.ToString()} to dialogue for player Id: {sessionId.ToString()}. dialog.Messages is null. Message was not sent."
            );
            return;
        }

        var notification = new WsChatMessageReceived
        {
            EventType = NotificationEventType.new_message,
            EventIdentifier = message.Id,
            DialogId = message.UserId,
            Message = message,
        };
        SendMessage(sessionId, notification);
    }

    /// <summary>
    ///     Helper function for SendMessageToPlayer(), get new dialog for storage in profile or find existing by sender id
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="messageType">Type of message to generate</param>
    /// <param name="senderDetails">Who is sending the message</param>
    /// <returns>Dialogue</returns>
    protected Models.Eft.Profile.Dialogue? GetDialog(MongoId sessionId, MessageType messageType, UserDialogInfo senderDetails)
    {
        // Use trader id if sender is trader, otherwise use nickname
        var dialogKey = senderDetails.Id;

        // Get all dialogs with pmcs/traders player has
        var dialogueData = saveServer.GetProfile(sessionId).DialogueRecords;

        // Ensure empty dialog exists based on sender details passed in
        if (dialogueData?.TryAdd(dialogKey, GetEmptyDialogTemplate(dialogKey, messageType, senderDetails)) ?? false)
        {
            return dialogueData[dialogKey];
        }

        logger.Error($"Could not add dialog key: {dialogKey.ToString()} to dialogueData for player Id: {sessionId.ToString()}.");
        return null;
    }

    /// <summary>
    ///     Get an empty dialog template
    /// </summary>
    /// <param name="dialogKey">Key to assign</param>
    /// <param name="messageType">Type of message</param>
    /// <param name="senderDetails">Sender details</param>
    /// <returns>Empty dialog template</returns>
    protected Models.Eft.Profile.Dialogue GetEmptyDialogTemplate(MongoId dialogKey, MessageType messageType, UserDialogInfo senderDetails)
    {
        return new Models.Eft.Profile.Dialogue
        {
            Id = dialogKey,
            Type = messageType,
            Messages = [],
            Pinned = false,
            New = 0,
            AttachmentsNew = 0,
            Users = senderDetails.Info?.MemberCategory == MemberCategory.Trader ? null : [senderDetails],
        };
    }
}
