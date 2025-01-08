using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class ProfileChangeNicknameRequestData
{
	[JsonPropertyName("nickname")]
	public string? Nickname { get; set; }
}