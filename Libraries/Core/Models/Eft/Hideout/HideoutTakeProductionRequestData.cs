using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutTakeProductionRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public int? Timestamp { get; set; }
}
