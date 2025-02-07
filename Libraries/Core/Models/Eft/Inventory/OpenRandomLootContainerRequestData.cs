using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record OpenRandomLootContainerRequestData : InventoryBaseActionRequestData
{
    /// <summary>
    ///     Container item id being opened
    /// </summary>
    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("to")]
    public List<ItemEvent.To>? To
    {
        get;
        set;
    }
}
