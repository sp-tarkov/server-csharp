using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutScavCaseStartRequestData : BaseInteractionRequestData
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
