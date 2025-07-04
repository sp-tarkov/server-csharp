using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutCancelProductionRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("recipeId")]
    public MongoId RecipeId { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
