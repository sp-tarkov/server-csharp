using System.Text.Json.Serialization;

namespace Core.Models.Eft.Notifier;

public class SelectProfileResponse
{
	[JsonPropertyName("status")]
	public string? Status { get; set; }
}