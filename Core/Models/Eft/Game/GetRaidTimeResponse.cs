using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record GetRaidTimeResponse
{
    [JsonPropertyName("RaidTimeMinutes")]
    public int? RaidTimeMinutes { get; set; }

    [JsonPropertyName("NewSurviveTimeSeconds")]
    public int? NewSurviveTimeSeconds { get; set; }

    [JsonPropertyName("OriginalSurvivalTimeSeconds")]
    public int? OriginalSurvivalTimeSeconds { get; set; }

    [JsonPropertyName("ExitChanges")]
    public List<ExtractChange>? ExitChanges { get; set; }
}

public record ExtractChange
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("MinTime")]
    public int? MinTime { get; set; }

    [JsonPropertyName("MaxTime")]
    public int? MaxTime { get; set; }

    [JsonPropertyName("Chance")]
    public int? Chance { get; set; }
}
