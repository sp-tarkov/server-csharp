using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public class WsNotificationEvent
{
    [JsonPropertyName("type")]
    public NotificationEventType? EventType { get; set; }

    [JsonPropertyName("eventId")]
    public string? EventIdentifier { get; set; }
}
