using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Hideout;

public class HideoutUpgradeRequestData 
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "HideoutUpgrade";
    
    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }
    
    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}