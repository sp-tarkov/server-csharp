using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutTakeItemOutRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "HideoutTakeItemsFromAreaSlots";

    [JsonPropertyName("areaType")]
    public int AreaType { get; set; }

    [JsonPropertyName("slots")]
    public int[] Slots { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}