using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

/// <summary>
///     New quest detail object for use with the CustomQuestService.<br/><br/>
///
/// It is recommended to build these objects manually rather than deserializing directly to it. This will keep you from
/// having to store locale data inside a quest file, or only having a single quest and associated locale in the same file. <br/><br/>
///
///
/// </summary>
public record NewQuestDetails
{
    /// <summary>
    ///     Quest to be added to the database
    /// </summary>
    public required Quest NewQuest { get; init; }

    /// <summary>
    ///     Locales for this quest. The primary key is the language to add to locale entries to<br/>
    /// The secondary key is the locale key, the value is the locale text itself.
    /// </summary>
    public required Dictionary<string, Dictionary<string, string>> Locales { get; init; }

    /// <summary>
    ///     Only Usec and Bear are valid entries here,
    /// if used it will lock that quest to only being available to that specific side.<br/><br/>
    ///
    /// If not used, this should be left null to keep the quest open to both Usec and Bears.
    /// </summary>
    public PlayerSide? LockedToSide { get; init; }
}

/// <summary>
///     Cloned quest object for cloning and modifying an existing quest.<br/><br/>
///
/// If you load after another mod, you can even clone a quest from that mod provided you know the Id.
/// </summary>
public record NewQuestFromCloneDetails
{
    /// <summary>
    ///     Id of the quest to copy and use as a base
    /// </summary>
    [JsonPropertyName("questTplToClone")]
    public required MongoId QuestTplToClone { get; set; }

    // TODO: Fill out
}

/// <summary>
///     Result from either creating a new quest or cloning one.
/// </summary>
public record CreateQuestResult
{
    // TODO: Find a way to shut this stupid inspection up. This CANNOT be a primary constructor you stupid ass linter, it would break equality.
    public CreateQuestResult(bool success, MongoId? questId, List<string>? errors)
    {
        Success = success;
        QuestId = questId;
        Errors = errors;
    }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("questId")]
    public MongoId? QuestId { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; }
}
