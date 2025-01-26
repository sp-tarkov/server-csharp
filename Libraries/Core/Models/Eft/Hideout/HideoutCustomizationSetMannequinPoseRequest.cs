using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutCustomizationSetMannequinPoseRequest : BaseInteractionRequestData
{
    [JsonPropertyName("poses")]
    public Dictionary<string, string>? Poses { get; set; }

    [JsonPropertyName("timestamp")]
    public double? Timestamp { get; set; }
}
