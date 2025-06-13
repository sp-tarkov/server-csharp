using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Location;

public record GetLocationRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("crc")]
    public int? Crc
    {
        get;
        set;
    }

    [JsonPropertyName("locationId")]
    public string? LocationId
    {
        get;
        set;
    }

    [JsonPropertyName("variantId")]
    public int? VariantId
    {
        get;
        set;
    }
}
