using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public record WsAid : WsNotificationEvent
{
    [JsonPropertyName("aid")]
    public int? Aid
    {
        get;
        set;
    }
}
