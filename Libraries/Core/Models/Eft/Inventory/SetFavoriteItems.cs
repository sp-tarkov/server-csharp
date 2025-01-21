using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record SetFavoriteItems : InventoryBaseActionRequestData
{
    [JsonPropertyName("items")]
    public List<object>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
