using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record GetItemPriceResult : MinMax<double>
{
    [JsonPropertyName("avg")]
    public double? Avg
    {
        get;
        set;
    }
}
