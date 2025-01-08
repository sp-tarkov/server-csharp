using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class CompletedAchievementsResponse
{
    [JsonPropertyName("elements")]
    public Dictionary<string, int>? Elements { get; set; }
}