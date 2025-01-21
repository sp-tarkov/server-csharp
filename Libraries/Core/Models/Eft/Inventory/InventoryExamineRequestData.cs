using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Inventory;

public record InventoryExamineRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("fromOwner")]
    public OwnerInfo? FromOwner { get; set; }
}
