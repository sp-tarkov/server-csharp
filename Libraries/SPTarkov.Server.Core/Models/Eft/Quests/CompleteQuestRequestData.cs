using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Quests;

public record CompleteQuestRequestData : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     Quest Id
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
