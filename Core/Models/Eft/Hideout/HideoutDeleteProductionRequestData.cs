using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutDeleteProductionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutDeleteProductionCommand";

    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public double? Timestamp { get; set; }
}