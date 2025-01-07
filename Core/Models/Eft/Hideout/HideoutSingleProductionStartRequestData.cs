using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutSingleProductionStartRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "HideoutSingleProductionStart";

    [JsonPropertyName("recipeId")]
    public string RecipeId { get; set; }

    [JsonPropertyName("items")]
    public List<HandoverItem> Items { get; set; }

    [JsonPropertyName("tools")]
    public List<HandoverItem> Tools { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

public class HandoverItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}