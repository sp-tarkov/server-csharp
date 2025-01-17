namespace Core.Models.Eft.Ws;

public record WsPing : WsNotificationEvent
{
    public WsPing()
    {
        EventType = NotificationEventType.ping;
        EventIdentifier = "ping";
    }
}
