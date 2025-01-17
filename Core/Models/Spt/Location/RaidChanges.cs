using System.Text.Json.Serialization;

namespace Core.Models.Spt.Location;

public record RaidChanges
{
    [JsonPropertyName("dynamicLootPercent")]
    public float? DynamicLootPercent { get; set; }

    [JsonPropertyName("staticLootPercent")]
    public float? StaticLootPercent { get; set; }

    [JsonPropertyName("simulatedRaidStartSeconds")]
    public int? SimulatedRaidStartSeconds { get; set; }
}
