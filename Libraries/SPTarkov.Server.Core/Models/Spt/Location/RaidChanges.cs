using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Location;

public record RaidChanges
{
    /// <summary>
    /// What percentage of dynamic loot should the map contain
    /// </summary>
    [JsonPropertyName("dynamicLootPercent")]
    public double? DynamicLootPercent
    {
        get;
        set;
    }

    /// <summary>
    /// What percentage of static loot should the map contain
    /// </summary>
    [JsonPropertyName("staticLootPercent")]
    public double? StaticLootPercent
    {
        get;
        set;
    }

    /// <summary>
    /// How many seconds into the raid is the player simulated to spawn in at
    /// </summary>
    [JsonPropertyName("simulatedRaidStartSeconds")]
    public double? SimulatedRaidStartSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// How many minutes are in the raid total
    /// </summary>
    [JsonPropertyName("raidTimeMinutes")]
    public double? RaidTimeMinutes
    {
        get;
        set;
    }

    /// <summary>
    /// The new number of seconds required to avoid a run through
    /// </summary>
    [JsonPropertyName("newSurviveTimeSeconds")]
    public double? NewSurviveTimeSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// The original number of seconds required to avoid a run through
    /// </summary>
    [JsonPropertyName("originalSurvivalTimeSeconds")]
    public double? OriginalSurvivalTimeSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Any changes required to the extract list
    /// </summary>
    [JsonPropertyName("exitChanges")]
    public List<ExtractChange>? ExitChanges
    {
        get;
        set;
    }
}

public record ExtractChange
{
    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("MinTime")]
    public double? MinTime
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTime")]
    public double? MaxTime
    {
        get;
        set;
    }

    [JsonPropertyName("Chance")]
    public double? Chance
    {
        get;
        set;
    }
}
