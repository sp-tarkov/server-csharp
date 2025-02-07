using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Ragfair;

public record GetMarketPriceRequestData : IRequestData
{
    [JsonPropertyName("templateId")]
    public string? TemplateId
    {
        get;
        set;
    }
}
