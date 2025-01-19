using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Insurance;

public record GetInsuranceCostRequestData : IRequestData
{
    [JsonPropertyName("traders")]
    public List<string>? Traders { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}
