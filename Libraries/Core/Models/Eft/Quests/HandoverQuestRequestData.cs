using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Quests;

public record HandoverQuestRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("qid")]
    public string? QuestId { get; set; }

    [JsonPropertyName("conditionId")]
    public string? ConditionId { get; set; }

    [JsonPropertyName("items")]
    public List<HandoverItem>? Items { get; set; }
}

public record HandoverItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
