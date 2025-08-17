using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryBindRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId Item { get; set; }

    [JsonPropertyName("index")]
    public string? Index { get; set; }
}
