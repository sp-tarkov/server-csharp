using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutContinuousProductionStartRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId
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

public record HideoutProperties
{
    public int? BtcFarmGcs
    {
        get;
        set;
    }

    public bool IsGeneratorOn
    {
        get;
        set;
    }

    public bool WaterCollectorHasFilter
    {
        get;
        set;
    }
}
