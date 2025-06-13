using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Services;

public record InsuranceEquipmentPkg
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("sessionID")]
    public string? SessionId
    {
        get;
        set;
    }

    [JsonPropertyName("pmcData")]
    public PmcData? PmcData
    {
        get;
        set;
    }

    [JsonPropertyName("itemToReturnToPlayer")]
    public Item? ItemToReturnToPlayer
    {
        get;
        set;
    }

    [JsonPropertyName("traderId")]
    public string? TraderId
    {
        get;
        set;
    }
}
