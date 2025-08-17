using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryMoveRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId? Item { get; set; }

    [JsonPropertyName("to")]
    public To? To { get; set; }
}
