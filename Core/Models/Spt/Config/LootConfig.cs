using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class LootConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-loot";

    /** Spawn positions to add into a map, key=mapid */
    [JsonPropertyName("looseLoot")]
    public Dictionary<string, Spawnpoint[]> LooseLoot { get; set; }

    /** Loose loot probability adjustments to apply on game start */
    [JsonPropertyName("looseLootSpawnPointAdjustments")]
    public Dictionary<string, Dictionary<string, double>> LooseLootSpawnPointAdjustments { get; set; }
}