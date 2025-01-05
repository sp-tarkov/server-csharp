using System.Text.Json.Serialization;

namespace Types.Models.Spt.Ragfair;

public class RagfairServerPrices
{
    [JsonPropertyName("static")]
    public Dictionary<string, int> Static { get; set; }
    
    [JsonPropertyName("dynamic")]
    public Dictionary<string, int> Dynamic { get; set; }
}