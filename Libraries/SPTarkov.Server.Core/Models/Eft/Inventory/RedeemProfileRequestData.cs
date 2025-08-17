using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Inventory;

public record RedeemProfileRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("events")]
    public List<RedeemProfileRequestEvent>? Events { get; set; }
}

public record RedeemProfileRequestEvent
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("MessageId")]
    public MongoId MessageId { get; set; }

    [JsonPropertyName("EventId")]
    public MongoId EventId { get; set; }
}
