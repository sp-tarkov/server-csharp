using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record ItemSpawnLimitSettings
{
    [JsonPropertyName("currentLimits")]
    public Dictionary<string, double>? CurrentLimits { get; set; }

    [JsonPropertyName("globalLimits")]
    public Dictionary<string, double>? GlobalLimits { get; set; }
}
