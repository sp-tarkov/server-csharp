using System.Text.Json.Serialization;
using Core.Models.Common;

namespace Core.Models.Eft.Ragfair;

public record GetItemPriceResult : MinMax
{
    [JsonPropertyName("avg")]
    public double? Avg
    {
        get;
        set;
    }
}
