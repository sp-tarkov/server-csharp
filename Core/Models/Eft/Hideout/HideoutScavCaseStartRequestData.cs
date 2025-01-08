using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutScavCaseStartRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutScavCaseProductionStart";

    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }

    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public class HideoutItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

public class Tool
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}