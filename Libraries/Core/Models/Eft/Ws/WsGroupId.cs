using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public record WsGroupId : WsNotificationEvent
{
    [JsonPropertyName("groupId")]
    public string? GroupId
    {
        get;
        set;
    }
}
