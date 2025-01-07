using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class OpenRandomLootContainerRequestData
{
    public string Action { get; set; } = "OpenRandomLootContainer";
    
    /// <summary>
    /// Container item id being opened
    /// </summary>
    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("to")]
    public List<ItemEvent.To> To { get; set; }
}