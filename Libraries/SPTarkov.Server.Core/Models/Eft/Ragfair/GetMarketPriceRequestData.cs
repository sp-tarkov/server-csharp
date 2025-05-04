using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetMarketPriceRequestData : IRequestData
{
    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }
}
