using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryTransferRequestData : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("item")]
    public MongoId Item { get; set; }

    [JsonPropertyName("with")]
    public MongoId? With { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
