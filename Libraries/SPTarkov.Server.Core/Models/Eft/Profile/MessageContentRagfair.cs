using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record MessageContentRagfair
{
    [JsonPropertyName("offerId")]
    public string? OfferId
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public double? Count
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
