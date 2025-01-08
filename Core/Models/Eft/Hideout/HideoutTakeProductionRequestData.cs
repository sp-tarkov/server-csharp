using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutTakeProductionRequestData {
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutTakeProduction";

    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public int? Timestamp { get; set; }
}