using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutCustomizationApplyRequestData: BaseInteractionRequestData
{

    /// <summary>
    /// Id of the newly picked item to apply to hideout
    /// </summary>
    [JsonPropertyName("offerId")]
    public string? OfferId { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
