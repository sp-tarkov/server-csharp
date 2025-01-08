using System.Text.Json.Serialization;
using Core.Models.Common;

namespace Core.Models.Eft.Ragfair;

public class GetItemPriceResult : MinMax
{
    [JsonPropertyName("avg")]
    public int? Avg { get; set; }
}