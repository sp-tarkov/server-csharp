using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GetRaidTimeResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("NewSurviveTimeSeconds")]
    public double? NewSurviveTimeSeconds { get; set; }

    [JsonPropertyName("OriginalSurvivalTimeSeconds")]
    public double? OriginalSurvivalTimeSeconds { get; set; }
}
