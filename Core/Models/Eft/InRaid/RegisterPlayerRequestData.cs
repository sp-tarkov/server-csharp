using System.Text.Json.Serialization;

namespace Core.Models.Eft.InRaid;

public class RegisterPlayerRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }
    
    [JsonPropertyName("locationId")]
    public string? LocationId { get; set; }
    
    [JsonPropertyName("variantId")]
    public int? VariantId { get; set; }
}