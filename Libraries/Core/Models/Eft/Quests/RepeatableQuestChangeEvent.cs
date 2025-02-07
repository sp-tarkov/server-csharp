using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Quests;

public record RepeatableQuestChangeRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("qid")]
    public string? QuestId
    {
        get;
        set;
    }
}
