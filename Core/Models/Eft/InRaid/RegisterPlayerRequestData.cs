using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.InRaid;

public class RegisterPlayerRequestData : IRequestData
{
    [JsonPropertyName("crc")]
    public int? Crc { get; set; }

    [JsonPropertyName("locationId")]
    public string? LocationId { get; set; }

    [JsonPropertyName("variantId")]
    public int? VariantId { get; set; }
}
