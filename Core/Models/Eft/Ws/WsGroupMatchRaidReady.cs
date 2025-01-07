using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public class WsGroupMatchRaidReady : WsNotificationEvent
{
	[JsonPropertyName("extendedProfile")]
	public GroupCharacter ExtendedProfile { get; set; }
}