using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public class WsGroupMatchInviteDecline : WsNotificationEvent
{
    [JsonPropertyName("aid")]
    public int Aid { get; set; }

    [JsonPropertyName("Nickname")]
    public string Nickname { get; set; }
}