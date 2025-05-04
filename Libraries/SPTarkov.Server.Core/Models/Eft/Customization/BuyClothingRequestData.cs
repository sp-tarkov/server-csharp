using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Customization;

public record BuyClothingRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("offer")]
    public string? Offer { get; set; }

    [JsonPropertyName("items")]
    public List<PaymentItemForClothing>? Items { get; set; }
}

public record PaymentItemForClothing
{
    [JsonPropertyName("del")]
    public bool? Del { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
