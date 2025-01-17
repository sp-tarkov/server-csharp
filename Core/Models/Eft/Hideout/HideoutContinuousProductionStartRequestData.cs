using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public record HideoutContinuousProductionStartRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; } = "HideoutContinuousProductionStart";

    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public double? Timestamp { get; set; }
}
