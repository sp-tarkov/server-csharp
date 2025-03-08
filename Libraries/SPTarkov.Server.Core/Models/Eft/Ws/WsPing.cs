namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsPing : WsNotificationEvent
{
    public WsPing()
    {
        EventType = NotificationEventType.ping;
        EventIdentifier = "ping";
    }
}
