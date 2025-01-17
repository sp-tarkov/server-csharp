using System.Text.Json.Serialization;

namespace Core.Models.Eft.Notifier;

public record SelectProfileResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
