using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutCancelProductionRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId
    {
        get;
        set;
    }

    [JsonPropertyName("timestamp")]
    public long? Timestamp
    {
        get;
        set;
    }
}
