using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Insurance;

public record InsureRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("tid")]
    public string? TransactionId
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<string>? Items
    {
        get;
        set;
    }
}
