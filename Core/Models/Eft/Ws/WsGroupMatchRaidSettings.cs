using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public class WsGroupMatchRaidSettings : WsNotificationEvent
{
	[JsonPropertyName("raidSettings")]
	public RaidSettings RaidSettings { get; set; }
}