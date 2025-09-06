using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutTakeProductionRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("recipeId")]
    public MongoId RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public int? Timestamp { get; set; }
}
