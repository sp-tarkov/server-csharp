using System.Text.Json.Serialization;
using Core.Models.Eft.Match;

namespace Core.Models.Eft.Ws;

public record WsGroupMatchInviteSend : WsNotificationEvent
{
    [JsonPropertyName("requestId")]
    public string? RequestId
    {
        get;
        set;
    }

    [JsonPropertyName("from")]
    public int? From
    {
        get;
        set;
    }

    [JsonPropertyName("members")]
    public List<GroupCharacter>? Members
    {
        get;
        set;
    }
}
