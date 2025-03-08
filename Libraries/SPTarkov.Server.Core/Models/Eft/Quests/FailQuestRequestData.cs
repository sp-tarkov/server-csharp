using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record FailQuestRequestData : InventoryBaseActionRequestData
{
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
