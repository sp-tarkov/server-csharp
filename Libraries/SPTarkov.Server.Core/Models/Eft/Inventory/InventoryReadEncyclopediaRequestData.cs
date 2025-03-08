using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryReadEncyclopediaRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("ids")]
    public List<string> Ids
    {
        get;
        set;
    }
}
