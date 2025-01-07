using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public class WsFriendsListAccept : WsNotificationEvent
{
	[JsonPropertyName("profile")]
	public SearchFriendResponse Profile { get; set; }
}