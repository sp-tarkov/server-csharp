using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record GetRaidTimeResponse
{
    [JsonPropertyName("RaidTimeMinutes")]
    public double? RaidTimeMinutes { get; set; }

    [JsonPropertyName("NewSurviveTimeSeconds")]
    public double? NewSurviveTimeSeconds { get; set; }

    [JsonPropertyName("OriginalSurvivalTimeSeconds")]
    public double? OriginalSurvivalTimeSeconds { get; set; }

    [JsonPropertyName("ExitChanges")]
    public List<ExtractChange>? ExitChanges { get; set; }
}

public record ExtractChange
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("MinTime")]
    public double? MinTime { get; set; }

    [JsonPropertyName("MaxTime")]
    public double? MaxTime { get; set; }

    [JsonPropertyName("Chance")]
    public double? Chance { get; set; }
}
