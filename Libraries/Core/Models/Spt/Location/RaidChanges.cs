using System.Text.Json.Serialization;

namespace Core.Models.Spt.Location;

public record RaidChanges
{
    [JsonPropertyName("dynamicLootPercent")]
    public double? DynamicLootPercent
    {
        get;
        set;
    }

    [JsonPropertyName("staticLootPercent")]
    public double? StaticLootPercent
    {
        get;
        set;
    }

    [JsonPropertyName("simulatedRaidStartSeconds")]
    public double? SimulatedRaidStartSeconds
    {
        get;
        set;
    }

    /** How many minutes are in the raid total */
    [JsonPropertyName("RaidTimeMinutes")]
    public double? RaidTimeMinutes
    {
        get;
        set;
    }

    /** The new number of seconds required to avoid a run through */
    [JsonPropertyName("NewSurviveTimeSeconds")]
    public double? NewSurviveTimeSeconds
    {
        get;
        set;
    }

    /** The original number of seconds required to avoid a run through */
    [JsonPropertyName("OriginalSurvivalTimeSeconds")]
    public double? OriginalSurvivalTimeSeconds
    {
        get;
        set;
    }

    /** Any changes required to the extract list */
    [JsonPropertyName("ExitChanges")]
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
