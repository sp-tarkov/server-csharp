using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutContinuousProductionStartRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public double? Timestamp { get; set; }
}

public record HideoutProperties
{
    public int? BtcFarmGcs { get; set; }
    public bool IsGeneratorOn { get; set; }
    public bool WaterCollectorHasFilter  { get; set; }
}
