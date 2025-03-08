using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsAidNickname : WsNotificationEvent
{
    [JsonPropertyName("aid")]
    public int? Aid
    {
        get;
        set;
    }

    [JsonPropertyName("Nickname")]
    public string? Nickname
    {
        get;
        set;
    }
}
