using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record LootConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-loot";

    /// <summary>
    ///     Spawn positions to add into a map, key=mapid
    /// </summary>
    [JsonPropertyName("looseLoot")]
    public required Dictionary<string, Spawnpoint[]> LooseLoot
    {
        get;
        set;
    }

    /// <summary>
    ///     Loose loot probability adjustments to apply on game start
    /// </summary>
    [JsonPropertyName("looseLootSpawnPointAdjustments")]
    public required Dictionary<string, Dictionary<string, double>> LooseLootSpawnPointAdjustments
    {
        get;
        set;
    }
}
