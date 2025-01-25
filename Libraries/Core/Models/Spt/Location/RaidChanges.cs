using System.Text.Json.Serialization;

namespace Core.Models.Spt.Location;

public record RaidChanges
{
    [JsonPropertyName("dynamicLootPercent")]
    public double? DynamicLootPercent { get; set; }

    [JsonPropertyName("staticLootPercent")]
    public double? StaticLootPercent { get; set; }

    [JsonPropertyName("simulatedRaidStartSeconds")]
    public double? SimulatedRaidStartSeconds { get; set; }
}
