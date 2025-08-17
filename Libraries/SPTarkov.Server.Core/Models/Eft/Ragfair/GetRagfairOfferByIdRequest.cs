using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetRagfairOfferByIdRequest
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("id")]
    public int? Id { get; set; }
}
