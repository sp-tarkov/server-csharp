using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RepeatableQuest : Quest
{
    [JsonPropertyName("changeCost")]
    public List<ChangeCost?>? ChangeCost { get; set; }

    [JsonPropertyName("changeStandingCost")]
    public int? ChangeStandingCost { get; set; }

    [JsonPropertyName("sptRepatableGroupName")]
    public string? SptRepatableGroupName { get; set; }

    [JsonPropertyName("questStatus")]
    public RepeatableQuestStatus? QuestStatus { get; set; }
}

public record RepeatableQuestDatabase
{
    [JsonPropertyName("templates")]
    public RepeatableTemplates? Templates { get; set; }

    [JsonPropertyName("rewards")]
    public RewardOptions? Rewards { get; set; }

    [JsonPropertyName("data")]
    public Options? Data { get; set; }

    [JsonPropertyName("samples")]
    public List<SampleQuests?>? Samples { get; set; }
}

public record RepeatableQuestStatus
{
    [JsonPropertyName("id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("qid")]
    public MongoId QId { get; set; }

    [JsonPropertyName("startTime")]
    public long? StartTime { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("statusTimers")]
    public object? StatusTimers { get; set; } // Use object for any type
}

public record RepeatableTemplates
{
    [JsonPropertyName("Elimination")]
    public RepeatableQuest? Elimination { get; set; }

    [JsonPropertyName("Completion")]
    public RepeatableQuest? Completion { get; set; }

    [JsonPropertyName("Exploration")]
    public RepeatableQuest? Exploration { get; set; }

    [JsonPropertyName("Pickup")]
    public RepeatableQuest? Pickup { get; set; }
}

public record PmcDataRepeatableQuest
{
    [JsonPropertyName("id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("unavailableTime")]
    public string? UnavailableTime { get; set; }

    [JsonPropertyName("activeQuests")]
    public List<RepeatableQuest>? ActiveQuests { get; set; }

    [JsonPropertyName("inactiveQuests")]
    public List<RepeatableQuest>? InactiveQuests { get; set; }

    [JsonPropertyName("endTime")]
    public long? EndTime { get; set; }

    /// <summary>
    ///     What it costs to reset: QuestId, ChangeRequirement. Redundant to change requirements within RepeatableQuest
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("changeRequirement")]
    public Dictionary<MongoId, ChangeRequirement?>? ChangeRequirement { get; set; }

    [JsonPropertyName("freeChanges")]
    public int? FreeChanges { get; set; }

    [JsonPropertyName("freeChangesAvailable")]
    public int? FreeChangesAvailable { get; set; }
}

public record ChangeRequirement
{
    [JsonPropertyName("changeCost")]
    public List<ChangeCost?>? ChangeCost { get; set; }

    [JsonPropertyName("changeStandingCost")]
    public double? ChangeStandingCost { get; set; }
}

public record ChangeCost
{
    /// <summary>
    ///     What item it will take to reset daily
    /// </summary>
    [JsonPropertyName("templateId")]
    public MongoId TemplateId { get; set; }

    /// <summary>
    ///     Amount of item needed to reset
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

// Config Options

public record RewardOptions
{
    [JsonPropertyName("itemsBlacklist")]
    public List<string>? ItemsBlacklist { get; set; }
}

public record Options
{
    [JsonPropertyName("Completion")]
    public CompletionFilter? Completion { get; set; }
}

public record CompletionFilter
{
    [JsonPropertyName("itemsBlacklist")]
    public List<ItemsBlacklist>? ItemsBlacklist { get; set; }

    [JsonPropertyName("itemsWhitelist")]
    public List<ItemsWhitelist>? ItemsWhitelist { get; set; }
}

public record ItemsBlacklist
{
    [JsonPropertyName("minPlayerLevel")]
    public int? MinPlayerLevel { get; set; }

    [JsonPropertyName("itemIds")]
    public HashSet<MongoId>? ItemIds { get; set; }
}

public record ItemsWhitelist
{
    [JsonPropertyName("minPlayerLevel")]
    public int? MinPlayerLevel { get; set; }

    [JsonPropertyName("itemIds")]
    public HashSet<MongoId>? ItemIds { get; set; }
}

public record SampleQuests
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("isKey")]
    public bool? IsKey { get; set; }

    [JsonPropertyName("restartable")]
    public bool? Restartable { get; set; }

    [JsonPropertyName("instantComplete")]
    public bool? InstantComplete { get; set; }

    [JsonPropertyName("secretQuest")]
    public bool? SecretQuest { get; set; }

    [JsonPropertyName("canShowNotificationsInGame")]
    public bool? CanShowNotificationsInGame { get; set; }

    [JsonPropertyName("rewards")]
    public Dictionary<string, List<Reward>>? Rewards { get; set; }

    [JsonPropertyName("conditions")]
    public QuestConditionTypes? Conditions { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("successMessageText")]
    public string? SuccessMessageText { get; set; }

    [JsonPropertyName("failMessageText")]
    public string? FailMessageText { get; set; }

    [JsonPropertyName("startedMessageText")]
    public string? StartedMessageText { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }
}
