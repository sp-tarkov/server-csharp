using System.Text.Json.Serialization;

namespace Types.Models.Spt.Ragfair;

public class TplWithFleaPrice
{
    [JsonPropertyName("tpl")]
    public string Tpl { get; set; }
    
    // Roubles
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}