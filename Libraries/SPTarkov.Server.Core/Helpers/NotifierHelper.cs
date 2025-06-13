using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class NotifierHelper(
    HttpServerHelper httpServerHelper,
    HashUtil hashUtil)
{
    protected static WsPing ping = new();

    public WsNotificationEvent GetDefaultNotification()
    {
        return ping;
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
        return new WsRagfairOfferSold
        {
            EventType = NotificationEventType.RagfairOfferSold,
            EventIdentifier = dialogueMessage.Id,
            OfferId = ragfairData.OfferId,
            HandbookId = ragfairData.HandbookId,
            Count = (int) ragfairData.Count
        };
    }

    /**
     * Create a new notification with the specified dialogueMessage object
     * @param dialogueMessage
     * @returns
     */
    public WsChatMessageReceived CreateNewMessageNotification(Message dialogueMessage)
    {
        return new WsChatMessageReceived
        {
            EventType = NotificationEventType.new_message,
            EventIdentifier = dialogueMessage.Id,
            DialogId = dialogueMessage.UserId,
            Message = dialogueMessage
        };
    }

    public WsRagfairNewRating CreateRagfairNewRatingNotification(double rating, bool isGrowing)
    {
        return new WsRagfairNewRating
        {
            EventType = NotificationEventType.RagfairNewRating,
            EventIdentifier = hashUtil.Generate(),
            Rating = rating,
            IsRatingGrowing = isGrowing
        };
    }

    public string GetWebSocketServer(string sessionId)
    {
        return $"{httpServerHelper.GetWebsocketUrl()}/notifierServer/getwebsocket/{sessionId}";
    }
}
