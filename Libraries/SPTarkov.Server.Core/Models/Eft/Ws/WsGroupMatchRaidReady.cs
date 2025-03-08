using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Match;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsGroupMatchRaidReady : WsNotificationEvent
{
    [JsonPropertyName("extendedProfile")]
    public GroupCharacter? ExtendedProfile
    {
        get;
        set;
    }
}
