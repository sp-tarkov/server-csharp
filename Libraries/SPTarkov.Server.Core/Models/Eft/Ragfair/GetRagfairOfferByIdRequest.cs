using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetRagfairOfferByIdRequest
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}
