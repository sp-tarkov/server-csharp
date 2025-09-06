using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryExamineRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public MongoId ItemId { get; set; }
}
