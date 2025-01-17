using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public record SetFavoriteItems : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "SetFavoriteItems";

    [JsonPropertyName("items")]
    public List<object>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
