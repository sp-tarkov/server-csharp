using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Ragfair;

public record RagfairServerPrices
{
    [JsonPropertyName("staticPrices")]
    public Dictionary<string, double>? StaticPrices { get; set; }

    [JsonPropertyName("dynamicPrices")]
    public Dictionary<string, double>? DynamicPrices { get; set; }
}
