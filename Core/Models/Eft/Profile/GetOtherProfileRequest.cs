using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class GetOtherProfileRequest 
{
	[JsonPropertyName("accountId")]
	public string AccountId { get; set; }
}