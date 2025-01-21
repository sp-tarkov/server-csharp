using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutImproveAreaRequestData : BaseInteractionRequestData
{
    /** Hideout area id from areas.json */
    [JsonPropertyName("id")]
    public string? AreaId { get; set; }

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
