using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public record EndOfflineRaidRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }

    [JsonPropertyName("exitStatus")]
    public string? ExitStatus { get; set; }

    [JsonPropertyName("exitName")]
    public string? ExitName { get; set; }

    [JsonPropertyName("raidSeconds")]
    public int? RaidSeconds { get; set; }
}
