using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record InventoryReadEncyclopediaRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "ReadEncyclopedia";

    [JsonPropertyName("ids")]
    public List<string>? Ids { get; set; }
}
