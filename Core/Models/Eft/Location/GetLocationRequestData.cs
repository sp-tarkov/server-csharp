using System.Text.Json.Serialization;

namespace Core.Models.Eft.Location;

public class GetLocationRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }

    [JsonPropertyName("locationId")]
    public string? LocationId { get; set; }

    [JsonPropertyName("variantId")]
    public int? VariantId { get; set; }
}
