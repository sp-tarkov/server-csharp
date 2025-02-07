using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Hideout;

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
