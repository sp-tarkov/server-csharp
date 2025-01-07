using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class ProfileChangeVoiceRequestData 
{
	[JsonPropertyName("voice")]
	public string Voice { get; set; }
}