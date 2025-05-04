using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutSingleProductionStartRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("items")]
    public List<IdWithCount>? Items { get; set; }

    [JsonPropertyName("tools")]
    public List<IdWithCount>? Tools { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
