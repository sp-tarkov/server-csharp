using Core.Annotations;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ws;
using Core.Models.Enums;
using Core.Servers;
using Core.Servers.Ws;
using Core.Services;
using Core.Utils;

namespace Core.Helpers;

[Injectable]
public class NotificationSendHelper
{
    private readonly IWebSocketConnectionHandler _sptWebSocketConnectionHandler;
    private readonly HashUtil _hashUtil;
    private readonly SaveServer _saveServer;
    private readonly NotificationService _notificationService;

    public NotificationSendHelper(
        IWebSocketConnectionHandler sptWebSocketConnectionHandler,
        HashUtil hashUtil,
        SaveServer saveServer,
        NotificationService notificationService
        )
    {
        _sptWebSocketConnectionHandler = sptWebSocketConnectionHandler;
        _hashUtil = hashUtil;
        _saveServer = saveServer;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Send notification message to the appropriate channel
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="notificationMessage"></param>
    public void SendMessage(string sessionID, WsNotificationEvent notificationMessage)
    {
        if (_sptWebSocketConnectionHandler.IsWebSocketConnected(sessionID))
        {
            _sptWebSocketConnectionHandler.SendMessageAsync(sessionID, notificationMessage).Wait();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}
