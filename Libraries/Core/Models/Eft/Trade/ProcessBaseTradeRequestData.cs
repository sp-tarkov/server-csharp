using System.Text.Json.Serialization;

namespace Core.Models.Eft.Trade;

public record ProcessBaseTradeRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("tid")]
    public string? TransactionId { get; set; }
}
