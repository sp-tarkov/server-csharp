using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutCircleOfCultistProductionStartRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
