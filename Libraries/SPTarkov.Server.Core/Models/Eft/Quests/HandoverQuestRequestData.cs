using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

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
