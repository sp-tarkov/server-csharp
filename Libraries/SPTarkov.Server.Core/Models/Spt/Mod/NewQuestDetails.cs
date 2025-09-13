using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

/// <summary>
///     New quest detail object for use with the CustomQuestService.
/// </summary>
public record NewQuestDetails
{
    /// <summary>
    ///     Quest to be added to the database
    /// </summary>
    [JsonPropertyName("newQuest")]
    public required Quest NewQuest { get; init; }

    /// <summary>
    ///     Locales for this quest. The primary key is the language to add to locale entries to<br/>
    /// The secondary key is the locale key, the value is the locale text itself.
    /// </summary>
    [JsonPropertyName("locales")]
    public required Dictionary<string, Dictionary<string, string>> Locales { get; init; }

    /// <summary>
    ///     Only Usec and Bear are valid entries here,
    /// if used it will lock that quest to only being available to that specific side.<br/><br/>
    ///
    /// If not used, this should be left null to keep the quest open to both Usec and Bears.
    /// </summary>
    [JsonPropertyName("lockedToSide")]
    public PlayerSide? LockedToSide { get; init; }
}

/// <summary>
///     Result from either creating a new quest or cloning one.
/// </summary>
public record CreateQuestResult(bool Success, MongoId? QuestId)
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = Success;

    [JsonPropertyName("questId")]
    public MongoId? QuestId { get; set; } = QuestId;

    [JsonPropertyName("errors")]
    public List<string> Errors { get; } = [];
}
