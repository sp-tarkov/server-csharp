using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Trade;

public class SellScavItemsToFenceRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "SellAllFromSavage";

    [JsonPropertyName("totalValue")]
    public double? TotalValue { get; set; }

    [JsonPropertyName("fromOwner")]
    public OwnerInfo? FromOwner { get; set; }

    [JsonPropertyName("toOwner")]
    public OwnerInfo? ToOwner { get; set; }
}