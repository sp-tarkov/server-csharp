using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryAddRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId? Item { get; set; }

    [JsonPropertyName("container")]
    public Container? Container { get; set; }
}
