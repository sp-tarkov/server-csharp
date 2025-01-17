using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public record SendRagfairReportRequestData
{
    [JsonPropertyName("offerId")]
    public int? OfferId { get; set; }
}
