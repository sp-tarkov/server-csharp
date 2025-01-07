using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutContinuousProductionStartRequestData 
{
    [JsonPropertyName("Action")]
    public string Action { get; } = "HideoutContinuousProductionStart";
    
    [JsonPropertyName("recipeId")]
    public string RecipeId { get; set; }
    
    [JsonPropertyName("timestamp")]
    public double Timestamp { get; set; }
}