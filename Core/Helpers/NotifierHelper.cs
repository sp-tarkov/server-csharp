using Core.Annotations;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ws;

namespace Core.Helpers;

[Injectable]
public class NotifierHelper
{
    private readonly HttpServerHelper _httpServerHelper;

    public NotifierHelper(
        HttpServerHelper httpServerHelper)
    {
        _httpServerHelper = httpServerHelper;
    }
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
        return new WsRagfairOfferSold{
            EventType = NotificationEventType.RagfairOfferSold,
            EventIdentifier = dialogueMessage.Id,
            OfferId = ragfairData.OfferId,
            HandbookId = ragfairData.HandbookId,
            Count = ragfairData.Count
        };
    }

    /**
     * Create a new notification with the specified dialogueMessage object
     * @param dialogueMessage
     * @returns
     */
    public WsChatMessageReceived CreateNewMessageNotification(Message dialogueMessage)
    {
        return new WsChatMessageReceived {
            EventType = NotificationEventType.new_message,
            EventIdentifier = dialogueMessage.Id,
            DialogId = dialogueMessage.UserId,
            Message = dialogueMessage,
        };
    }

    public string GetWebSocketServer(string sessionID)
    {
        return $"{ _httpServerHelper.GetWebsocketUrl()}/ notifierServer / getwebsocket /{sessionID}";
    }
}
