using System.Text.Json.Serialization;
using Core.Models.Eft.Match;

namespace Core.Models.Eft.Ws;

public record WsGroupMatchRaidReady : WsNotificationEvent
{
    [JsonPropertyName("extendedProfile")]
    public GroupCharacter? ExtendedProfile { get; set; }
}
