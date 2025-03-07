using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsNotificationEvent
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationEventType? EventType
    {
        get;
        set;
    }

    [JsonPropertyName("eventId")]
    public string? EventIdentifier
    {
        get;
        set;
    }
}
