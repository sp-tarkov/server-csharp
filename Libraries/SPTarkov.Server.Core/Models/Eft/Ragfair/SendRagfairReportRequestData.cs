using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record SendRagfairReportRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("offerId")]
    public int? OfferId { get; set; }
}
