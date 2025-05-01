using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record StartLocalRaidRequestData : IRequestData
{
    [JsonPropertyName("serverId")]
    public string? ServerId
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public string? Location
    {
        get;
        set;
    }

    [JsonPropertyName("timeVariant")]
    public string? TimeVariant
    {
        get;
        set;
    }

    [JsonPropertyName("mode")]
    public string? Mode
    {
        get;
        set;
    }

    [JsonPropertyName("playerSide")]
    public string? PlayerSide
    {
        get;
        set;
    }

    [JsonPropertyName("transitionType")]
    public TransitionType? TransitionType
    {
        get;
        set;
    }

    [JsonPropertyName("transition")]
    public Transition? Transition
    {
        get;
        set;
    }

    /// <summary>
    ///     Should loot generation be skipped, default false
    /// </summary>
    [JsonPropertyName("sptSkipLootGeneration")]
    public bool? ShouldSkipLootGeneration
    {
        get;
        set;
    }
}
