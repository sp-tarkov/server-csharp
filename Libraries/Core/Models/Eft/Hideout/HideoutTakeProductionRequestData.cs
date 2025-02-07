using System.Text.Json.Serialization;
using Core.Models.Eft.Inventory;

namespace Core.Models.Eft.Hideout;

public record HideoutTakeProductionRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId
    {
        get;
        set;
    }

    [JsonPropertyName("timestamp")]
    public int? Timestamp
    {
        get;
        set;
    }
}
