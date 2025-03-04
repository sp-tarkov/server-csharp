using System.Text.Json.Serialization;
using Core.Models.Eft.Common;

namespace Core.Models.Spt.Config;

public record LootConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-loot";

    /// <summary>
    /// Spawn positions to add into a map, key=mapid
    /// </summary>
    [JsonPropertyName("looseLoot")]
    public Dictionary<string, Spawnpoint[]> LooseLoot
    {
        get;
        set;
    }

    /// <summary>
    /// Loose loot probability adjustments to apply on game start
    /// </summary>
    [JsonPropertyName("looseLootSpawnPointAdjustments")]
    public Dictionary<string, Dictionary<string, double>>? LooseLootSpawnPointAdjustments
    {
        get;
        set;
    }
}
