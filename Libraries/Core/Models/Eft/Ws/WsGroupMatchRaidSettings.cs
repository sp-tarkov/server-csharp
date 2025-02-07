using System.Text.Json.Serialization;
using Core.Models.Eft.Match;

namespace Core.Models.Eft.Ws;

public record WsGroupMatchRaidSettings : WsNotificationEvent
{
    [JsonPropertyName("raidSettings")]
    public RaidSettings? RaidSettings
    {
        get;
        set;
    }
}
