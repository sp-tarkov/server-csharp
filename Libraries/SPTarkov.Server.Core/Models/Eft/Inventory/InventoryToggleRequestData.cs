using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryToggleRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId? Item { get; set; }

    [JsonPropertyName("value")]
    public bool? Value { get; set; }
}
