using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Location
{
    /// <summary>
    ///     Map meta-data
    /// </summary>
    [JsonPropertyName("base")]
    public LocationBase? Base
    {
        get;
        set;
    }

    /// <summary>
    ///     Loose loot positions and item weights
    /// </summary>
    [JsonPropertyName("looseLoot")]
    public LazyLoad<LooseLoot>? LooseLoot
    {
        get;
        set;
    }

    /// <summary>
    ///     Static loot item weights
    /// </summary>
    [JsonPropertyName("staticLoot")]
    public LazyLoad<Dictionary<string, StaticLootDetails>>? StaticLoot
    {
        get;
        set;
    }

    /// <summary>
    ///     Static container positions and item weights
    /// </summary>
    [JsonPropertyName("staticContainers")]
    public LazyLoad<StaticContainerDetails>? StaticContainers
    {
        get;
        set;
    }

    [JsonPropertyName("staticAmmo")]
    public Dictionary<string, List<StaticAmmoDetails>> StaticAmmo
    {
        get;
        set;
    }

    /// <summary>
    ///     All possible static containers on map + their assign groupings
    /// </summary>
    [JsonPropertyName("statics")]
    public StaticContainer? Statics
    {
        get;
        set;
    }

    /// <summary>
    ///     All possible map extracts
    /// </summary>
    [JsonPropertyName("allExtracts")]
    public Exit[] AllExtracts
    {
        get;
        set;
    }
}
