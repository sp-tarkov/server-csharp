using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record RedeemProfileRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("events")]
    public List<RedeemProfileRequestEvent>? Events { get; set; }
}

public record RedeemProfileRequestEvent
{
    [JsonPropertyName("MessageId")]
    public string? MessageId { get; set; }

    [JsonPropertyName("EventId")]
    public string? EventId { get; set; }
}
