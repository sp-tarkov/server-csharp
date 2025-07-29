using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record FailQuestRequestData : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("qid")]
    public MongoId QuestId { get; set; }

    [JsonPropertyName("removeExcessItems")]
    public bool? RemoveExcessItems { get; set; }
}
