using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Achievement
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("index")]
    public required int Index { get; set; }

    [JsonPropertyName("id")]
    public required MongoId Id { get; set; }

    [JsonPropertyName("imageUrl")]
    public required string ImageUrl { get; set; }

    /// <summary>
    /// Unused in the client
    /// </summary>
    [JsonPropertyName("assetPath")]
    public string? AssetPath { get; set; }

    [JsonPropertyName("rewards")]
    public required IEnumerable<Reward> Rewards { get; set; }

    [JsonPropertyName("conditions")]
    public required AchievementQuestConditionTypes Conditions { get; set; }

    /// <summary>
    /// Unused in the client
    /// </summary>
    [JsonPropertyName("showProgress")]
    public bool? ShowProgress { get; set; }

    [JsonPropertyName("rarity")]
    public required string Rarity { get; set; }

    [JsonPropertyName("hidden")]
    public required bool Hidden { get; set; }

    [JsonPropertyName("showConditions")]
    public required bool ShowConditions { get; set; }

    [JsonPropertyName("progressBarEnabled")]
    public required bool ProgressBarEnabled { get; set; }

    [JsonPropertyName("side")]
    public required string Side { get; set; }
}

public record AchievementQuestConditionTypes
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("started")]
    public List<QuestCondition>? Started { get; set; }

    [JsonPropertyName("availableForFinish")]
    public List<QuestCondition>? AvailableForFinish { get; set; }

    [JsonPropertyName("availableForStart")]
    public List<QuestCondition>? AvailableForStart { get; set; }

    [JsonPropertyName("success")]
    public List<QuestCondition>? Success { get; set; }

    [JsonPropertyName("fail")]
    public List<QuestCondition> Fail { get; set; }
}
