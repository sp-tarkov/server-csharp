using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutCircleOfCultistProductionStartRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
