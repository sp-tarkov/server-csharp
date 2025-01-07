using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutImproveAreaRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "HideoutImproveArea";

    /** Hideout area id from areas.json */
    [JsonPropertyName("id")]
    public string AreaId { get; set; }

    [JsonPropertyName("areaType")]
    public int AreaType { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem> Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}