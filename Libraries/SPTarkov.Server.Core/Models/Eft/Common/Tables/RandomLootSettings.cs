using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RandomLootSettings
{
    [JsonPropertyName("allowToSpawnIdenticalItems")]
    public bool? AllowToSpawnIdenticalItems
    {
        get;
        set;
    }

    [JsonPropertyName("allowToSpawnQuestItems")]
    public bool? AllowToSpawnQuestItems
    {
        get;
        set;
    }

    [JsonPropertyName("countByRarity")]
    public List<object>? CountByRarity
    {
        get;
        set;
    } // TODO: object here

    [JsonPropertyName("excluded")]
    public RandomLootExcluded? Excluded
    {
        get;
        set;
    }

    [JsonPropertyName("filters")]
    public List<object>? Filters
    {
        get;
        set;
    } // TODO: object here

    [JsonPropertyName("findInRaid")]
    public bool? FindInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("maxCount")]
    public double? MaxCount
    {
        get;
        set;
    }

    [JsonPropertyName("minCount")]
    public double? MinCount
    {
        get;
        set;
    }
}
