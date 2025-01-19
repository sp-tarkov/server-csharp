using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public record WsGroupMatchLeaderChanged : WsNotificationEvent
{
    [JsonPropertyName("owner")]
    public int? Owner { get; set; }
}
