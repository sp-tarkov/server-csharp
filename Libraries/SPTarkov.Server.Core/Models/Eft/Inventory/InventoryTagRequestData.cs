using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record InventoryTagRequestData : InventoryBaseActionRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("item")]
    public MongoId? Item { get; set; }

    [JsonPropertyName("TagName")]
    public string? TagName { get; set; }

    [JsonPropertyName("TagColor")]
    public int? TagColor { get; set; }
}
