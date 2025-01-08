using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutCustomizationApplyRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutCustomizationApply";

    /// <summary>
    /// Id of the newly picked item to apply to hideout
    /// </summary>
    [JsonPropertyName("offerId")]
    public string? OfferId { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}