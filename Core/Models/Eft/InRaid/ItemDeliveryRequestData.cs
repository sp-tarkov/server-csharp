using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.InRaid;

public class ItemDeliveryRequestData
{
    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }
}
