using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutScavCaseStartRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }

    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public record HideoutItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

public record Tool
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}
