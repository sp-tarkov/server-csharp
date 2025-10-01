using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Insurance;

public record InsureRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("tid")]
    public MongoId TransactionId { get; set; }

    [JsonPropertyName("items")]
    public List<MongoId>? Items { get; set; }
}
