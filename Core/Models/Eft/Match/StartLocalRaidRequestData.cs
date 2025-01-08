using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Match;

public class StartLocalRaidRequestData
{
    [JsonPropertyName("serverId")]
    public string? ServerId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("timeVariant")]
    public string? TimeVariant { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("playerSide")]
    public string? PlayerSide { get; set; }

    [JsonPropertyName("transitionType")]
    public TransitionType? TransitionType { get; set; }

    [JsonPropertyName("transition")]
    public Transition? Transition { get; set; }

    /** Should loot generation be skipped, default false */
    [JsonPropertyName("sptSkipLootGeneration")]
    public bool? ShouldSkipLootGeneration { get; set; }
}