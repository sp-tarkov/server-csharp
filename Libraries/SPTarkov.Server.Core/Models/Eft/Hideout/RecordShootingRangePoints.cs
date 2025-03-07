using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record RecordShootingRangePoints : InventoryBaseActionRequestData
{
    [JsonPropertyName("points")]
    public int? Points
    {
        get;
        set;
    }
}
