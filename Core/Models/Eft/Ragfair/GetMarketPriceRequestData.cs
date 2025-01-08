using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public class GetMarketPriceRequestData
{
    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }
}