using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ws;

public record WsRagfairOfferSold : WsNotificationEvent
{
    [JsonPropertyName("offerId")]
    public string? OfferId
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }

    [JsonPropertyName("handbookId")]
    public string? HandbookId
    {
        get;
        set;
    }
}
