using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsProfileChangeEvent : WsNotificationEvent
{
    [JsonPropertyName("Changes")]
    public Dictionary<string, double?> Changes { get; set; }
}
