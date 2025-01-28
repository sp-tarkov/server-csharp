using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Hideout;

public record RecordShootingRangePoints : InventoryBaseActionRequestData
{
    [JsonPropertyName("points")]
    public int? Points { get; set; }
}
