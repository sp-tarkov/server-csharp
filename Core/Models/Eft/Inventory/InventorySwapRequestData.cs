using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Inventory;

public class InventorySwapRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Swap";
    
    [JsonPropertyName("item")]
    public string? Item { get; set; }
    
    [JsonPropertyName("to")]
    public To? To { get; set; }
    
    [JsonPropertyName("item2")]
    public string? Item2 { get; set; }
    
    [JsonPropertyName("to2")]
    public To? To2 { get; set; }
    
    [JsonPropertyName("fromOwner2")]
    public OwnerInfo? FromOwner2 { get; set; }
    
    [JsonPropertyName("toOwner2")]
    public OwnerInfo? ToOwner2 { get; set; }
}