using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record SendRagfairReportRequestData
{
    [JsonPropertyName("offerId")]
    public int? OfferId
    {
        get;
        set;
    }
}
