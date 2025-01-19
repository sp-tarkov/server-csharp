using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public record CreateProfileResponse
{
    [JsonPropertyName("uid")]
    public string? UserId { get; set; }
}
