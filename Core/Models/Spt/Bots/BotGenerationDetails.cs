using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Enums;

namespace Core.Models.Spt.Bots;

public class BotGenerationDetails
{
    /// <summary>
    /// Should the bot be generated as a PMC
    /// </summary>
    [JsonPropertyName("isPmc")]
    public bool? IsPmc { get; set; }

    /// <summary>
    /// assault/pmcBot etc
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Side of bot
    /// </summary>
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    /// <summary>
    /// Active players current level
    /// </summary>
    [JsonPropertyName("playerLevel")]
    public int? PlayerLevel { get; set; }

    [JsonPropertyName("playerName")]
    public string? PlayerName { get; set; }

    /// <summary>
    /// Level specific overrides for PMC level
    /// </summary>
    [JsonPropertyName("locationSpecificPmcLevelOverride")]
    public MinMax? LocationSpecificPmcLevelOverride { get; set; }

    /// <summary>
    /// Delta of highest level of bot e.g. 50 means 50 levels above player
    /// </summary>
    [JsonPropertyName("botRelativeLevelDeltaMax")]
    public int? BotRelativeLevelDeltaMax { get; set; }

    /// <summary>
    /// Delta of lowest level of bot e.g. 50 means 50 levels below player
    /// </summary>
    [JsonPropertyName("botRelativeLevelDeltaMin")]
    public int? BotRelativeLevelDeltaMin { get; set; }

    /// <summary>
    /// How many to create and store
    /// </summary>
    [JsonPropertyName("botCountToGenerate")]
    public int? BotCountToGenerate { get; set; }

    /// <summary>
    /// Desired difficulty of the bot
    /// </summary>
    [JsonPropertyName("botDifficulty")]
    public string? BotDifficulty { get; set; }

    /// <summary>
    /// Will the generated bot be a player scav
    /// </summary>
    [JsonPropertyName("isPlayerScav")]
    public bool? IsPlayerScav { get; set; }

    [JsonPropertyName("eventRole")]
    public string? EventRole { get; set; }

    [JsonPropertyName("allPmcsHaveSameNameAsPlayer")]
    public bool? AllPmcsHaveSameNameAsPlayer { get; set; }
}
