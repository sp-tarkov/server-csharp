using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Quests;

public record HandoverQuestRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("qid")]
    public string? QuestId
    {
        get;
        set;
    }

    [JsonPropertyName("conditionId")]
    public string? ConditionId
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<IdWithCount>? Items
    {
        get;
        set;
    }
}
