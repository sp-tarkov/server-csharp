using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public record HideoutDeleteProductionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutDeleteProductionCommand";

    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public double? Timestamp { get; set; }
}
