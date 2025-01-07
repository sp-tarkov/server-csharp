using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class CreateProfileResponse
{
	[JsonPropertyName("uid")]
	public string UserId { get; set; }
}