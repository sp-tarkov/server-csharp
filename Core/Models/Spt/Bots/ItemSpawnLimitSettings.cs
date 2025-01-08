using System.Text.Json.Serialization;

namespace Core.Models.Spt.Bots;

public class ItemSpawnLimitSettings
{
    [JsonPropertyName("currentLimits")]
    public Dictionary<string, int> CurrentLimits { get; set; }

    [JsonPropertyName("globalLimits")]
    public Dictionary<string, int> GlobalLimits { get; set; }
}