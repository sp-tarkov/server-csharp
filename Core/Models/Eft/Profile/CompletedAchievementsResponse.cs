using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public record CompletedAchievementsResponse
{
    [JsonPropertyName("elements")]
    public Dictionary<string, int>? Elements { get; set; }
}
