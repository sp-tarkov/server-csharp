using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutToggleAreaRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
