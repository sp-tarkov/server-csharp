using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public class WsGroupMatchInviteSend : WsNotificationEvent
{
	[JsonPropertyName("requestId")]
	public string RequestId { get; set; }

	[JsonPropertyName("from")]
	public int From { get; set; }

	[JsonPropertyName("members")]
	public List<GroupCharacter> Members { get; set; }
}