using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Inventory;

public record PinOrLockItemRequest : InventoryBaseActionRequestData
{
    /// <summary>
    /// Id of item being pinned
    /// </summary>
    [JsonPropertyName("Item")]
    public string? Item
    {
        get;
        set;
    }

    /// <summary>
    /// "Pinned"/"Locked"/"Free"
    /// </summary>
    [JsonPropertyName("State")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PinLockState? State
    {
        get;
        set;
    }
}
