using System.Text.Json.Serialization;

namespace Core.Models.Spt.Ragfair;

public record TplWithFleaPrice
{
    [JsonPropertyName("tpl")]
    public string? Tpl { get; set; }

    // Roubles
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
}
