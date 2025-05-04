using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Insurance;

public record GetInsuranceCostRequestData : IRequestData
{
    [JsonPropertyName("traders")]
    public List<string>? Traders { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}
