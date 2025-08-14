using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class NotifierHelper(HttpServerHelper httpServerHelper)
{
    protected static readonly WsPing Ping = new();

    public WsNotificationEvent GetDefaultNotification()
    {
        return Ping;
    }

    /// <summary>
    /// Create a new notification that displays the "Your offer was sold!" prompt and removes sold offer from "My Offers" on clientside
    /// </summary>
    /// <param name="dialogueMessage">Message from dialog that was sent</param>
    /// <param name="ragfairData">Ragfair data to attach to notification</param>
    /// <returns></returns>
    public WsRagfairOfferSold CreateRagfairOfferSoldNotification(Message dialogueMessage, MessageContentRagfair ragfairData)
    {
        return new WsRagfairOfferSold
        {
            EventType = NotificationEventType.RagfairOfferSold,
            EventIdentifier = dialogueMessage.Id,
            OfferId = ragfairData.OfferId,
            HandbookId = ragfairData.HandbookId,
            Count = Convert.ToInt32(ragfairData.Count),
        };
    }

    /// <summary>
    /// Create a new notification with the specified dialogueMessage object
    /// </summary>
    /// <param name="dialogueMessage"></param>
    /// <returns>WsChatMessageReceived</returns>
    public WsChatMessageReceived CreateNewMessageNotification(Message dialogueMessage)
    {
        return new WsChatMessageReceived
        {
            EventType = NotificationEventType.new_message,
            EventIdentifier = dialogueMessage.Id,
            DialogId = dialogueMessage.UserId,
            Message = dialogueMessage,
        };
    }

    /// <summary>
    /// Create a new rating ragfair notification
    /// </summary>
    /// <param name="rating">new rating</param>
    /// <param name="isGrowing">Rating is going up</param>
    /// <returns>WsRagfairNewRating</returns>
    public WsRagfairNewRating CreateRagfairNewRatingNotification(double rating, bool isGrowing)
    {
        return new WsRagfairNewRating
        {
            EventType = NotificationEventType.RagfairNewRating,
            EventIdentifier = new MongoId(),
            Rating = rating,
            IsRatingGrowing = isGrowing,
        };
    }

    /// <summary>
    /// Get the web socket server URI
    /// </summary>
    /// <param name="sessionId">Player/Session id</param>
    /// <returns>URI as string</returns>
    public string GetWebSocketServer(MongoId sessionId)
    {
        return $"{httpServerHelper.GetWebsocketUrl()}/notifierServer/getwebsocket/{sessionId.ToString()}";
    }
}
