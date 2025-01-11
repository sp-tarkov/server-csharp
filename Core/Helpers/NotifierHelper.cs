using Core.Annotations;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ws;

namespace Core.Helpers;

[Injectable]
public class NotifierHelper
{
    public WsNotificationEvent GetDefaultNotification()
    {
        throw new NotImplementedException();
    }

    /**
     * Create a new notification that displays the "Your offer was sold!" prompt and removes sold offer from "My Offers" on clientside
     * @param dialogueMessage Message from dialog that was sent
     * @param ragfairData Ragfair data to attach to notification
     * @returns
     */
    public WsRagfairOfferSold CreateRagfairOfferSoldNotification(
        Message dialogueMessage,
        MessageContentRagfair ragfairData)
    {
        throw new NotImplementedException();
    }

    /**
     * Create a new notification with the specified dialogueMessage object
     * @param dialogueMessage
     * @returns
     */
    public WsChatMessageReceived CreateNewMessageNotification(Message dialogueMessage)
    {
        throw new NotImplementedException();
    }

    public string GetWebSocketServer(string sessionID)
    {
        throw new NotImplementedException();
    }
}
