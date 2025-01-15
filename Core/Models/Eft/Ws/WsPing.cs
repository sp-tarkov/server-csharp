namespace Core.Models.Eft.Ws;

public class WsPing : WsNotificationEvent
{
    public WsPing()
    {
        EventType = NotificationEventType.ping;
        EventIdentifier = "ping";
    }
}
