using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record PinOrLockItemRequest : InventoryBaseActionRequestData
{
    /**
     * Id of item being pinned
     */
    [JsonPropertyName("Item")]
    public string? Item
    {
        get;
        set;
    }

    /**
     * "Pinned"/"Locked"/"Free"
     */
    [JsonPropertyName("State")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PinLockState? State
    {
        get;
        set;
    }
}
