using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetMarketPriceRequestData : IRequestData
{
    [JsonPropertyName("templateId")]
    public MongoId TemplateId { get; set; }
}
