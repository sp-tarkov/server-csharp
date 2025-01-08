using System.Text.Json.Serialization;
using Core.Models.Eft.Profile;

namespace Core.Models.Eft.Ws;

public class WsFriendsListAccept : WsNotificationEvent
{
	[JsonPropertyName("profile")]
	public SearchFriendResponse? Profile { get; set; }
}