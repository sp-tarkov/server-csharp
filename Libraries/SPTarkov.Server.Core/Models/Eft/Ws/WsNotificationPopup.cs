using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsNotificationPopup : WsNotificationEvent
{
    [JsonPropertyName("eventId")]
    public MongoId EventId { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; }

    [JsonPropertyName("message")]
    public MongoId Message { get; set; }
}
