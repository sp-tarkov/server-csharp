using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public record WsNotificationEvent
{
    [JsonPropertyName("type")]
    public NotificationEventType? EventType { get; set; }

    [JsonPropertyName("eventId")]
    public string? EventIdentifier { get; set; }
}
