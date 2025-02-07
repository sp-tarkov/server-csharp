using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Quests;

public record CompleteQuestRequestData : InventoryBaseActionRequestData
{
    /**
     * Quest Id
     */
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
