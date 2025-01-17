using System.Text.Json.Serialization;

namespace Core.Models.Eft.Insurance;

public record GetInsuranceCostRequestData
{
    [JsonPropertyName("traders")]
    public List<string>? Traders { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}
