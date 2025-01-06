using System.Text.Json.Serialization;

namespace Types.Models.Spt.Inventory;

public class OwnerInventoryItems
{
    [JsonPropertyName("from")]
    public List<Item> From { get; set; }
    
    [JsonPropertyName("to")]
    public List<Item> To { get; set; }
    
    [JsonPropertyName("sameInventory")]
    public bool SameInventory { get; set; }
    
    [JsonPropertyName("isMail")]
    public bool IsMail { get; set; }
}