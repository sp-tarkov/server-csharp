using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsGroupId : WsNotificationEvent
{
    [JsonPropertyName("groupId")]
    public string? GroupId
    {
        get;
        set;
    }
}
