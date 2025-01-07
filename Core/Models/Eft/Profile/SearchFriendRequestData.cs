using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class SearchFriendRequestData
{
	[JsonPropertyName("nickname")]
	public string Nickname { get; set; }
}