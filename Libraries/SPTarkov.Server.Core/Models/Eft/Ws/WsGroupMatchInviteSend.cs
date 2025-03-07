using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Match;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

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
