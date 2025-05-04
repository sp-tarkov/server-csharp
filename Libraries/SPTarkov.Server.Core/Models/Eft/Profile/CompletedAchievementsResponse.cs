using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record CompletedAchievementsResponse
{
    [JsonPropertyName("elements")]
    public Dictionary<string, int>? Elements { get; set; }
}
