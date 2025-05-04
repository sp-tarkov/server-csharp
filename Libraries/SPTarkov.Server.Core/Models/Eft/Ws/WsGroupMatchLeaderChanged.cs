using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsGroupMatchLeaderChanged : WsNotificationEvent
{
    [JsonPropertyName("owner")]
    public int? Owner { get; set; }
}
