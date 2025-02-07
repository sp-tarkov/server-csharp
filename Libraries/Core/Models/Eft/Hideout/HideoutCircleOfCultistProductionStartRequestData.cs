using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Hideout;

public record HideoutCircleOfCultistProductionStartRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("timestamp")]
    public long? Timestamp
    {
        get;
        set;
    }
}
