using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record CompletedAchievementsResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("elements")]
    public Dictionary<string, int>? Elements
    {
        get;
        set;
    }
}
