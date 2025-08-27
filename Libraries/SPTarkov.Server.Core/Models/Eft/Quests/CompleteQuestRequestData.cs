using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record CompleteQuestRequestData : InventoryBaseActionRequestData
{
    /// <summary>
    ///     Quest Id
    /// </summary>
    [JsonPropertyName("qid")]
    public MongoId QuestId { get; set; }

    [JsonPropertyName("removeExcessItems")]
    public bool? RemoveExcessItems { get; set; }

    /// <summary>
    /// This is only set if the quest is repeatable
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
