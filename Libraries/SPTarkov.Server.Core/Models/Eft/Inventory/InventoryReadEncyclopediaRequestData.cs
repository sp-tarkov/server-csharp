using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryReadEncyclopediaRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("ids")]
    public List<MongoId> Ids { get; set; }
}
