using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutDeleteProductionRequestData : InventoryBaseActionRequestData
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
