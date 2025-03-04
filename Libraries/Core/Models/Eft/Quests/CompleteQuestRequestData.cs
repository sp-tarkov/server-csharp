using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Quests;

public record CompleteQuestRequestData : InventoryBaseActionRequestData
{
    /// <summary>
    /// Quest Id
    /// </summary>
    [JsonPropertyName("qid")]
    public string? QuestId
    {
        get;
        set;
    }

    [JsonPropertyName("removeExcessItems")]
    public bool? RemoveExcessItems
    {
        get;
        set;
    }
}
