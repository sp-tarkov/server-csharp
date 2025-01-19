using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public record GetMarketPriceRequestData
{
    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }
}
