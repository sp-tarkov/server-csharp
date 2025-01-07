using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutUpgradeCompleteRequestData
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "HideoutUpgradeComplete";

    [JsonPropertyName("areaType")]
    public int AreaType { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}