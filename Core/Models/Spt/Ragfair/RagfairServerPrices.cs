using System.Text.Json.Serialization;

namespace Core.Models.Spt.Ragfair;

public class RagfairServerPrices
{
    [JsonPropertyName("static")]
    public Dictionary<string, int> Static { get; set; }
    
    [JsonPropertyName("dynamic")]
    public Dictionary<string, int> Dynamic { get; set; }
}