using System.Text.Json.Serialization;

namespace Core.Models.Eft.Inventory;

public class RedeemProfileRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "RedeemProfileReward";

    [JsonPropertyName("events")]
    public List<RedeemProfileRequestEvent> Events { get; set; }
}

public class RedeemProfileRequestEvent
{
    [JsonPropertyName("MessageId")]
    public string MessageId { get; set; }

    [JsonPropertyName("EventId")]
    public string EventId { get; set; }
}