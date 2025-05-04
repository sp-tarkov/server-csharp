using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Match;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsGroupMatchRaidSettings : WsNotificationEvent
{
    [JsonPropertyName("raidSettings")]
    public RaidSettings? RaidSettings { get; set; }
}
