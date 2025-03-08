using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutCustomizationSetMannequinPoseRequest : InventoryBaseActionRequestData
{
    [JsonPropertyName("poses")]
    public Dictionary<string, string>? Poses
    {
        get;
        set;
    }

    [JsonPropertyName("timestamp")]
    public double? Timestamp
    {
        get;
        set;
    }
}
