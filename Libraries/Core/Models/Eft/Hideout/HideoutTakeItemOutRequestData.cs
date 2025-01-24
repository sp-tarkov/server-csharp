using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public record HideoutTakeItemOutRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("slots")]
    public List<int>? Slots { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
