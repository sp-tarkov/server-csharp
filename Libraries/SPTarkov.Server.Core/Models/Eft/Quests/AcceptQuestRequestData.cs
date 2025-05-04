using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record AcceptQuestRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("qid")]
    public string? QuestId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
