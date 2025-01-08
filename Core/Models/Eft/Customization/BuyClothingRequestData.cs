using System.Text.Json.Serialization;

namespace Core.Models.Eft.Customization;

public class BuyClothingRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "CustomizationBuy";

    [JsonPropertyName("offer")]
    public string? Offer { get; set; }

    [JsonPropertyName("items")]
    public List<PaymentItemForClothing>? Items { get; set; }
}

public class PaymentItemForClothing
{
    [JsonPropertyName("del")]
    public bool? Del { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}