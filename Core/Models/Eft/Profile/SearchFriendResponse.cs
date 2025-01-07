using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class SearchFriendResponse
{
	[JsonPropertyName("_id")]
	public string Id { get; set; }

	[JsonPropertyName("aid")]
	public int Aid { get; set; }

	[JsonPropertyName("Info")]
	public FriendInfo Info { get; set; }
}

// NOTE: Renamed from `Info` because of a name collision.
public class FriendInfo
{
	[JsonPropertyName("Nickname")]
	public string Nickname { get; set; }

	[JsonPropertyName("Side")]
	public string Side { get; set; }

	[JsonPropertyName("Level")]
	public int Level { get; set; }

	[JsonPropertyName("MemberCategory")]
	public int MemberCategory { get; set; }

	[JsonPropertyName("SelectedMemberCategory")]
	public int SelectedMemberCategory { get; set; }
}