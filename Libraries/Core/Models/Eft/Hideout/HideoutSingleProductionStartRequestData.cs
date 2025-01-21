using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutSingleProductionStartRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("items")]
    public List<HandoverItem>? Items { get; set; }

    [JsonPropertyName("tools")]
    public List<HandoverItem>? Tools { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public record HandoverItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
