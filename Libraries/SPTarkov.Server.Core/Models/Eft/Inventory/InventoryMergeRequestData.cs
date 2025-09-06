using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryMergeRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId Item { get; set; }

    [JsonPropertyName("with")]
    public MongoId? With { get; set; }
}
