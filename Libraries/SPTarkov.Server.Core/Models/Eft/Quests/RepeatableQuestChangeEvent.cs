using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record RepeatableQuestChangeRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("qid")]
    public string? QuestId { get; set; }
}
