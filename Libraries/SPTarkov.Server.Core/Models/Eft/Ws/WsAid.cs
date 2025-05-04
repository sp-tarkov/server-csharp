using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsAid : WsNotificationEvent
{
    [JsonPropertyName("aid")]
    public int? Aid { get; set; }
}
