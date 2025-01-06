using System.Text.Json.Serialization;

namespace Types.Models.Spt.Location;

public class RaidChanges
{
    [JsonPropertyName("dynamicLootPercent")]
    public float DynamicLootPercent { get; set; }
    
    [JsonPropertyName("staticLootPercent")]
    public float StaticLootPercent { get; set; }
    
    [JsonPropertyName("simulatedRaidStartSeconds")]
    public int SimulatedRaidStartSeconds { get; set; }
}